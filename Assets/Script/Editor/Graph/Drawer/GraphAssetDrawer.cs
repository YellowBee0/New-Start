using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using YBFramework.Bridge.Data;
using Object = UnityEngine.Object;

namespace YBFramework.Editor.Graph
{
    public sealed class GraphAssetDrawer : IUndoRedoRecorder
    {
        private GraphAsset m_GraphAsset;

        private CustomGraphView m_GraphView;

        private EdgeConnectorListener m_EdgeConnectorListener;

        private SerializedObject m_SO;

        private SerializedProperty m_NodeDataListProperty;

        private readonly List<BaseNodeDrawer> m_NodeDrawers = new();

        public GraphAsset GetGraphAsset()
        {
            return m_GraphAsset;
        }

        public CustomGraphView GetGraphView()
        {
            return m_GraphView;
        }

        public void SetPortViewEdgeConnector(PortView portView)
        {
            portView.SetEdgeConnector(new EdgeConnector<EdgeView>(m_EdgeConnectorListener));
        }

        public SerializedObject GetSO()
        {
            return m_SO;
        }

        public SerializedProperty GetNodeDataProperty(BaseNodeData nodeData)
        {
            IReadOnlyList<BaseNodeData> nodesData = m_GraphAsset.GetNodesData();
            for (int i = 0; i < nodesData.Count; i++)
            {
                if (nodesData[i] == nodeData)
                {
                    return m_NodeDataListProperty.GetArrayElementAtIndex(i);
                }
            }
            return null;
        }

        public IReadOnlyList<BaseNodeDrawer> GetNodeDrawers()
        {
            return m_NodeDrawers;
        }

        public void AddNodeDrawer(BaseNodeDrawer nodeDrawer)
        {
            m_NodeDrawers.Add(nodeDrawer);
            m_GraphView.AddNodeView(nodeDrawer.GetNodeView());
        }

        /// <summary>
        /// 移除一个NodeDrawer
        /// 移除后调用Release释放的原因：本身BaseNodeDrawer并没有其他作用，仅在创建GraphAsset树状视图和检索有用，如果被移除树状结构基本可以理解为BaseNodeDrawer被释放了
        /// </summary>
        /// <param name="nodeDrawer">移除的NodeDrawer</param>
        public void RemoveNodeDrawer(BaseNodeDrawer nodeDrawer)
        {
            if (m_NodeDrawers.Remove(nodeDrawer))
            {
                BaseNodeDrawer.Release(nodeDrawer);
            }
            m_GraphView.RemoveNodeView(nodeDrawer.GetNodeView());
        }

        public BaseNodeDrawer FindNodeDrawer(int nodeID)
        {
            for (int i = 0; i < m_NodeDrawers.Count; i++)
            {
                BaseNodeDrawer nodeDrawer = m_NodeDrawers[i];
                if (nodeDrawer.GetNodeData().GetNodeID() == nodeID)
                {
                    return nodeDrawer;
                }
            }
            return null;
        }

        public void DrawGraphView(GraphAsset graphAsset)
        {
            m_GraphAsset = graphAsset;
            m_SO = new SerializedObject(graphAsset);
            m_NodeDataListProperty = m_SO.FindProperty("m_NodesData");
            m_GraphView = CustomGraphView.Allocate(this);
            m_EdgeConnectorListener = new EdgeConnectorListener(this);
            m_GraphView.graphViewChanged += OnGraphViewChanged;
            IReadOnlyList<BaseNodeData> nodesData = graphAsset.GetNodesData();
            for (int i = 0; i < nodesData.Count; i++)
            {
                BaseNodeData nodeData = nodesData[i];
                BaseNodeDrawer nodeDrawer = BaseNodeDrawer.Allocate(nodeData.GetType());
                if (nodeDrawer != null)
                {
                    nodeDrawer.DrawNodeView(this, nodeData);
                    AddNodeDrawer(nodeDrawer);
                }
            }

            IReadOnlyList<NodeView> nodeViews = m_GraphView.GetNodeViews();
            for (int i = 0; i < nodeViews.Count; i++)
            {
                nodeViews[i].RevertPortViewsConnection();
            }
        }

        public void SetDirty()
        {
            EditorUtility.SetDirty(m_GraphAsset);
        }

        private void OnEdgeConnect(Edge edge)
        {
            PortView inputPortView = (PortView)edge.input;
            PortView outputPortView = (PortView)edge.output;
            BasePortData inputPortData = inputPortView.GetPortDrawer().GetPortData();
            BasePortData outputPortData = outputPortView.GetPortDrawer().GetPortData();
            bool isInputCanConnectOutput = inputPortData.CanConnect(outputPortData);
            bool isOutputCanConnectInput = outputPortData.CanConnect(inputPortData);
            if (!(isInputCanConnectOutput ^ isOutputCanConnectInput))
            {
                if (isInputCanConnectOutput)
                {
                    Debug.LogError("This can not know witch one to connect because of both port can connect to other port");
                }
                return;
            }
            UndoRedoBehaviourManager.BeginRecord("Connect port");
            if (inputPortView.capacity == Port.Capacity.Single)
            {
                foreach (Edge connection in inputPortView.connections)
                {
                    if (connection != edge)
                    {
                        DisconnectEdge(edge);
                        break;
                    }
                }
            }
            if (outputPortView.capacity == Port.Capacity.Single)
            {
                foreach (Edge connection in outputPortView.connections)
                {
                    if (connection != edge)
                    {
                        DisconnectEdge(edge);
                        break;
                    }
                }
            }
            EdgeView edgeView = (EdgeView)edge;
            PortView fromPortView;
            PortView toPortView;
            if (isInputCanConnectOutput)
            {
                fromPortView = inputPortView;
                toPortView = outputPortView;
            }
            else
            {
                fromPortView = outputPortView;
                toPortView = inputPortView;
            }
            ConnectEdge(fromPortView, toPortView, edgeView);
            UndoRedoBehaviourManager.EndRecord();
            SetDirty();
        }

        private GraphViewChange OnGraphViewChanged(GraphViewChange changeData)
        {
            UndoRedoBehaviourManager.BeginRecord("Remove data or move node view");
            if (changeData.elementsToRemove != null)
            {
                for (int i = changeData.elementsToRemove.Count - 1; i >= 0; i--)
                {
                    switch (changeData.elementsToRemove[i])
                    {
                        case NodeView nodeView:
                            BaseNodeData nodeData = nodeView.GetNodeDrawer().GetNodeData();
                            m_GraphAsset.RemoveNodeData(nodeData);
                            RemoveNodeDrawer(nodeView.GetNodeDrawer());
                            //记录Undo行为
                            NodeViewUndoRedoBehaviour nodeViewUndoRedo = IUndoRedoBehaviour.Allocate<NodeViewUndoRedoBehaviour>();
                            nodeViewUndoRedo.Initialize(nodeData.GetNodeID(), false);
                            UndoRedoBehaviourManager.PushUndoRedoBehaviour(nodeViewUndoRedo);
                            changeData.elementsToRemove.RemoveAt(i);
                            break;
                        case EdgeView edgeView:
                            DisconnectEdge(edgeView);
                            changeData.elementsToRemove.RemoveAt(i);
                            break;
                    }
                }
            }
            if (changeData.movedElements != null)
            {
                for (int i = 0; i < changeData.movedElements.Count; i++)
                {
                    if (changeData.movedElements[i] is NodeView nodeView)
                    {
                        NodeViewPositionUndoRedoBehaviour positionUndoRedo = IUndoRedoBehaviour.Allocate<NodeViewPositionUndoRedoBehaviour>();
                        positionUndoRedo.Initialize(nodeView.GetNodeID(), changeData.moveDelta);
                        UndoRedoBehaviourManager.PushUndoRedoBehaviour(positionUndoRedo);
                        nodeView.GetNodeDrawer().GetNodeData().Position += changeData.moveDelta;
                    }
                }
            }
            UndoRedoBehaviourManager.EndRecord();
            SetDirty();
            return changeData;
        }

        private void ConnectEdge(PortView fromPortView, PortView toPortView, EdgeView edgeView)
        {
            BasePortData fromPortData = fromPortView.GetPortDrawer().GetPortData();
            BasePortData toPortData = toPortView.GetPortDrawer().GetPortData();
            //添加UndoRedo行为
            ConnectionUndoRedoBehaviour connectUndo = IUndoRedoBehaviour.Allocate<ConnectionUndoRedoBehaviour>();
            connectUndo.Initialize(fromPortData.GetNodeData().GetNodeID(), fromPortData.GetPortID(), toPortData.GetNodeData().GetNodeID(), toPortData.GetPortID(), true);
            UndoRedoBehaviourManager.PushUndoRedoBehaviour(connectUndo);
            //添加连接数据
            fromPortData.Connect(toPortData);
            //添加View连线
            CustomGraphView.Connect(fromPortView, toPortView, edgeView, m_GraphView);
        }

        private void DisconnectEdge(Edge edge)
        {
            EdgeView otherEdgeView = (EdgeView)edge;
            PortView fromPortView = otherEdgeView.GetFromPortView();
            PortView toPortView = otherEdgeView.GetToPortView();
            BasePortData fromPortData = fromPortView.GetPortDrawer().GetPortData();
            BasePortData toPortData = toPortView.GetPortDrawer().GetPortData();
            //添加UndoRedo行为
            ConnectionUndoRedoBehaviour disconnectUndo = IUndoRedoBehaviour.Allocate<ConnectionUndoRedoBehaviour>();
            disconnectUndo.Initialize(fromPortData.GetNodeData().GetNodeID(), fromPortData.GetPortID(), toPortData.GetNodeData().GetNodeID(), toPortData.GetPortID(), false);
            UndoRedoBehaviourManager.PushUndoRedoBehaviour(disconnectUndo);
            //删除连线数据
            fromPortData.Disconnect(toPortData);
            //删除View连线
            CustomGraphView.Disconnect(otherEdgeView, m_GraphView);
        }

        private void OnRelease()
        {
            m_GraphView.graphViewChanged -= OnGraphViewChanged;
            for (int i = 0; i < m_NodeDrawers.Count; i++)
            {
                BaseNodeDrawer.Release(m_NodeDrawers[i]);
            }
            m_NodeDrawers.Clear();
        }

        public Object GetRecordObject()
        {
            return m_GraphAsset;
        }

        public void OnBeginRecord()
        {
        }

        public void OnEndRecord()
        {
        }

        public void OnUndoRedo()
        {
            m_SO.Update();
        }

        #region Edge connector class

        private sealed class EdgeConnectorListener : IEdgeConnectorListener
        {
            private readonly GraphAssetDrawer m_GraphAssetDrawer;

            public EdgeConnectorListener(GraphAssetDrawer graphAssetDrawer)
            {
                m_GraphAssetDrawer = graphAssetDrawer;
            }

            public void OnDropOutsidePort(Edge edge, Vector2 position)
            {
            }

            public void OnDrop(GraphView graphView, Edge edge)
            {
                m_GraphAssetDrawer.OnEdgeConnect(edge);
            }
        }

        #endregion

        #region Pool

        private static readonly Stack<GraphAssetDrawer> s_Pool = new();

        public static GraphAssetDrawer Allocate()
        {
            return s_Pool.Count > 0 ? s_Pool.Pop() : new GraphAssetDrawer();
        }

        public static void Release(GraphAssetDrawer graphAssetDrawer)
        {
            graphAssetDrawer.OnRelease();
            s_Pool.Push(graphAssetDrawer);
        }

        #endregion
    }
}