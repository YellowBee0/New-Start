using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using YBFramework.Bridge.Data;

namespace YBFramework.Editor.Graph
{
    public sealed class GraphAssetDrawer
    {
        private GraphAsset m_GraphAsset;

        private CustomGraphView m_GraphView;

        private EdgeConnectorListener m_EdgeConnectorListener;

        private SerializedObject m_SO;

        private SerializedProperty m_NodeDataListProperty;

        private readonly List<BaseNodeDrawer> m_NodeDrawers = new();

        private int m_UndoGroupIndex;

        private bool m_IsModifyGraphAsset;

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

        /// <summary>
        /// 在修改GraphAsset数据之前调用此方法，以便支持Undo，并且在修改完成后调用ApplyModifyGraphAsset方法。
        /// </summary>
        /// <param name="undoName">本次Undo名</param>
        /// <exception cref="InvalidOperationException">如果调用了一次这个方法，没调用应用修改将会抛出异常</exception>
        public void ModifyGraphAsset(string undoName)
        {
            if (m_IsModifyGraphAsset)
            {
                throw new InvalidOperationException("You have already called ModifyGraphAsset, please call ApplyModifyGraphAsset before calling ModifyGraphAsset again.");
            }
            m_IsModifyGraphAsset = true;
            Undo.IncrementCurrentGroup();
            m_UndoGroupIndex = Undo.GetCurrentGroup();
            Undo.SetCurrentGroupName(undoName);
            Undo.RegisterCompleteObjectUndo(m_GraphAsset, undoName);
        }

        /// <summary>
        /// 应用修改GraphAsset数据，并且支持Undo。
        /// </summary>
        public void ApplyModifyGraphAsset()
        {
            if (m_IsModifyGraphAsset)
            {
                //TODO:是否要区分通过SerializedObject修改还是直接内存修改？
                // 通过SerializedObject修改会自己记录Undo，我需要通过UndoGroup区分
                EditorUtility.SetDirty(m_GraphAsset);
                Undo.CollapseUndoOperations(m_UndoGroupIndex);
                m_IsModifyGraphAsset = false;
            }
        }

        //这个函数上升到全局
        public void ClearModifyGraphAsset()
        {
            if (m_IsModifyGraphAsset)
            {
                throw new InvalidOperationException("Cannot clear modify graph asset while modifying, please call ApplyModifyGraphAsset before calling ClearModifyGraphAsset.");
            }
            Undo.ClearAll();
            UndoRedoBehaviourManager.Clear();
        }

        public void PushUndoRedoBehaviour(IUndoRedoBehaviour undoRedoBehaviour)
        {
            if (m_IsModifyGraphAsset)
            {
                UndoRedoBehaviourManager.PushUndoRedoBehaviour(m_UndoGroupIndex, undoRedoBehaviour);
            }
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
            ModifyGraphAsset("Connect port");
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
            //保存本次Undo数据
            ApplyModifyGraphAsset();
        }

        private GraphViewChange OnGraphViewChanged(GraphViewChange changeData)
        {
            ModifyGraphAsset("Remove data or move node view");
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
                            nodeViewUndoRedo.Initialize(this, nodeData.GetNodeID(), false);
                            PushUndoRedoBehaviour(nodeViewUndoRedo);
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
                        positionUndoRedo.Initialize(this, nodeView.GetNodeID(), changeData.moveDelta);
                        PushUndoRedoBehaviour(positionUndoRedo);
                        nodeView.GetNodeDrawer().GetNodeData().Position += changeData.moveDelta;
                    }
                }
            }
            ApplyModifyGraphAsset();
            return changeData;
        }

        private void ConnectEdge(PortView fromPortView, PortView toPortView, EdgeView edgeView)
        {
            BasePortData fromPortData = fromPortView.GetPortDrawer().GetPortData();
            BasePortData toPortData = toPortView.GetPortDrawer().GetPortData();
            //添加UndoRedo行为
            ConnectionUndoRedoBehaviour connectUndo = IUndoRedoBehaviour.Allocate<ConnectionUndoRedoBehaviour>();
            connectUndo.Initialize(this, fromPortData.GetNodeData().GetNodeID(), fromPortData.GetPortID(), toPortData.GetNodeData().GetNodeID(), toPortData.GetPortID(), true);
            PushUndoRedoBehaviour(connectUndo);
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
            disconnectUndo.Initialize(this, fromPortData.GetNodeData().GetNodeID(), fromPortData.GetPortID(), toPortData.GetNodeData().GetNodeID(), toPortData.GetPortID(), false);
            PushUndoRedoBehaviour(disconnectUndo);
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