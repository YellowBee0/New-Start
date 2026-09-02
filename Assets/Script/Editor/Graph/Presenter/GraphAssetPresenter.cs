using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using YBFramework.Bridge.Data;

namespace YBFramework.Editor.Graph
{
    public sealed class GraphAssetPresenter
    {
        private static readonly Stack<GraphAssetPresenter> s_GraphAssetPresenters = new();

        public static GraphAssetPresenter AllocateGraphAssetPresenter()
        {
            return s_GraphAssetPresenters.Count > 0 ? s_GraphAssetPresenters.Pop() : new GraphAssetPresenter();
        }

        public static void ReleaseGraphAssetPresenter(GraphAssetPresenter graphAssetPresenter)
        {
            graphAssetPresenter.OnRelease();
            s_GraphAssetPresenters.Push(graphAssetPresenter);
        }

        private GraphAsset m_GraphAsset;

        private SerializedObject m_SO;

        private SerializedProperty m_NodeDataListProperty;

        private CustomGraphView m_GraphView;

        private readonly List<BaseNodeDataPresenter> m_NodePresenters = new();

        private void OnRelease()
        {
            for (int i = 0; i < m_NodePresenters.Count; i++)
            {
                BaseNodeDataPresenter.ReleaseNodePresenter(m_NodePresenters[i]);
            }
            m_NodePresenters.Clear();
        }

        private void ShowNodeSearchView(NodeCreationContext context)
        {
            SearchWindow.Open(new SearchWindowContext(context.screenMousePosition), NodeSearchEntry.GetSearchEntry(m_GraphAsset.GraphType));
        }

        public void Initialize(GraphAsset graphAsset)
        {
            m_GraphAsset = graphAsset;
            m_SO = new SerializedObject(graphAsset);
            m_NodeDataListProperty = m_SO.FindProperty("m_NodesData");
            m_GraphView = new CustomGraphView(this)
            {
                nodeCreationRequest = ShowNodeSearchView
            };
            m_GraphView.graphViewChanged += OnGraphViewChanged;
            IReadOnlyList<BaseNodeData> nodesData = graphAsset.GetNodesData();
            for (int i = 0; i < nodesData.Count; i++)
            {
                BaseNodeData nodeData = nodesData[i];
                BaseNodeDataPresenter nodeDataPresenter = BaseNodeDataPresenter.AllocateNodePresenter(nodeData.GetType());
                if (nodeDataPresenter != null)
                {
                    nodeDataPresenter.Initialize(this, nodeData, m_NodeDataListProperty.GetArrayElementAtIndex(i));
                    AddNodeDataPresenter(nodeDataPresenter);
                }
            }

            //TODO:连线的时候有可能存在端口数据被意外删除，比如程序删除了某个节点的端口代码，导致序列化数据不存在这个端口，但是连线数据还在（还在的数据只有连接到删除端口的数据）；
            // 再比如代理辅助节点删除了一个端口代理，代理节点依旧保存了原始的数据。
            // 对此需要一个类或者集合用于保存这些意外删除的数据的连线，因为只能获取到连线数据，端口名、方向（运行时数据）都找不到了。
            for (int i = 0; i < m_NodePresenters.Count; i++)
            {
                BaseNodeDataPresenter fromNodeDataPresenter = m_NodePresenters[i];
                IReadOnlyList<BasePortDataPresenter> fromPortPresenters = fromNodeDataPresenter.GetPortPresenters();
                for (int j = 0; j < fromPortPresenters.Count; j++)
                {
                    BasePortDataPresenter fromPortDataPresenter = fromPortPresenters[j];
                    BasePortData fromPortData = fromPortDataPresenter.GetPortData();
                    int portConnectionDataCount = fromPortData.GetPortConnectionsDataCount();
                    for (int k = 0; k < portConnectionDataCount; k++)
                    {
                        PortConnectionData portConnectionData = fromPortData.PortConnectionDataOfIndex(k);
                        BaseNodeDataPresenter toNodeDataPresenter = FindNodeDataPresenter(portConnectionData.NodeID);
                        if (toNodeDataPresenter != null)
                        {
                            BasePortDataPresenter toPortDataPresenter = toNodeDataPresenter.FindPortDataPresenter(portConnectionData.PortID);
                            Edge edge = fromPortDataPresenter.GetPortView().ConnectTo(toPortDataPresenter.GetPortView());
                            m_GraphView.AddElement(edge);
                        }
                    }
                }
            }
        }

        public GraphAsset GetGraphAsset()
        {
            return m_GraphAsset;
        }

        public CustomGraphView GetGraphView()
        {
            return m_GraphView;
        }

        public IReadOnlyList<BaseNodeDataPresenter> GetNodeDataPresenters()
        {
            return m_NodePresenters;
        }

        public SerializedProperty GetNodeSerializedProperty(BaseNodeData nodeData)
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

        public BaseNodeDataPresenter FindNodeDataPresenter(int nodeID)
        {
            for (int i = 0; i < m_NodePresenters.Count; i++)
            {
                if (m_NodePresenters[i].GetNodeData().GetNodeID() == nodeID)
                {
                    return m_NodePresenters[i];
                }
            }
            return null;
        }

        public void AddNodeDataPresenter(BaseNodeDataPresenter nodeDataPresenter)
        {
            m_NodePresenters.Add(nodeDataPresenter);
            m_GraphView.AddElement(nodeDataPresenter.GetNodeView());
        }

        public void RemoveNodeDataPresenter(BaseNodeDataPresenter nodeDataPresenter)
        {
            if (m_NodePresenters.Remove(nodeDataPresenter))
            {
                m_GraphAsset.RemoveNodeData(nodeDataPresenter.GetNodeData());
                nodeDataPresenter.ClearPortDataPresenter();
                m_GraphView.RemoveElement(nodeDataPresenter.GetNodeView());
                BaseNodeDataPresenter.ReleaseNodePresenter(nodeDataPresenter);
            }
        }

        public void UpdateSO()
        {
            m_SO.Update();
        }

        private GraphViewChange OnGraphViewChanged(GraphViewChange changeValue)
        {
            //TODO:需要支持Undo
            //TODO:不要在这里调用GraphView的RemoveElement
            if (changeValue.elementsToRemove != null)
            {
                for (int i = 0; i < changeValue.elementsToRemove.Count; i++)
                {
                    switch (changeValue.elementsToRemove[i])
                    {
                        case NodeView nodeView:
                            RemoveNodeDataPresenter(nodeView.NodeDataPresenter);
                            break;
                        case Edge edge:
                        {
                            PortView inputPortView = (PortView)edge.input;
                            PortView outputPortView = (PortView)edge.output;
                            BasePortData inputPortData = inputPortView.PortDataDataPresenter.GetPortData();
                            BasePortData outputPortData = outputPortView.PortDataDataPresenter.GetPortData();
                            inputPortData.Disconnect(outputPortData);
                            outputPortData.Disconnect(inputPortData);
                            inputPortView.Disconnect(edge);
                            outputPortView.Disconnect(edge);
                            m_GraphView.RemoveElement(edge);
                            break;
                        }
                    }
                }
            }
            if (changeValue.movedElements != null)
            {
                for (int i = 0; i < changeValue.movedElements.Count; i++)
                {
                    if (changeValue.movedElements[i] is NodeView nodeView)
                    {
                        nodeView.NodeDataPresenter.GetNodeData().Position += changeValue.moveDelta;
                    }
                }
            }
            EditorUtility.SetDirty(m_GraphAsset);
            return changeValue;
        }
        
        public void OnEdgeConnect(Edge edge)
        {
            PortView inputPortView = (PortView)edge.input;
            PortView outputPortView = (PortView)edge.output;
            BasePortData inputPortData = inputPortView.PortDataDataPresenter.GetPortData();
            BasePortData outputPortData = outputPortView.PortDataDataPresenter.GetPortData();
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
            //TODO:输入输出单独处理自己的连线。
            Edge singleConnectionToRemove = null;
            if (inputPortView.capacity == Port.Capacity.Single)
            {
                foreach (Edge connection in inputPortView.connections)
                {
                    if (connection != edge)
                    {
                        singleConnectionToRemove = connection;
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
                        singleConnectionToRemove = connection;
                        break;
                    }
                }
            }
            if (singleConnectionToRemove != null)
            {
                PortView toRemoveInputPortView = (PortView)singleConnectionToRemove.input;
                PortView toRemoveOutputPortView = (PortView)singleConnectionToRemove.output;
                BasePortData toRemoveInputPortData = toRemoveInputPortView.PortDataDataPresenter.GetPortData();
                BasePortData toRemoveOutputPortData = toRemoveOutputPortView.PortDataDataPresenter.GetPortData();
                //区分不了这条连线是输入端口发起的连接还是输出端口发起的连接，所以直接调用两个的Disconnect
                toRemoveInputPortData.Disconnect(toRemoveOutputPortData);
                toRemoveOutputPortData.Disconnect(toRemoveInputPortData);
                //视图上是必须两个端口都调用Disconnect
                toRemoveInputPortView.Disconnect(singleConnectionToRemove);
                toRemoveOutputPortView.Disconnect(singleConnectionToRemove);
                m_GraphView.RemoveElement(singleConnectionToRemove);
            }
            if (isInputCanConnectOutput)
            {
                inputPortData.Connect(outputPortData);
            }
            else
            {
                outputPortData.Connect(inputPortData);
            }
            inputPortView.Connect(edge);
            outputPortView.Connect(edge);
            m_GraphView.AddElement(edge);
            EditorUtility.SetDirty(m_GraphAsset);
        }
    }
}