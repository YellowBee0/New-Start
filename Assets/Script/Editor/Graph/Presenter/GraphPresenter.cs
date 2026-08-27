using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using YBFramework.Bridge.Data;

namespace YBFramework.Editor.Graph.Presenter
{
    public sealed class GraphPresenter
    {
        private static readonly Stack<GraphPresenter> s_GraphPresenters = new();

        public static GraphPresenter AllocateGraphPresenter()
        {
            return s_GraphPresenters.Count > 0 ? s_GraphPresenters.Pop() : new GraphPresenter();
        }

        public static void ReleaseGraphPresenter(GraphPresenter graphPresenter)
        {
            s_GraphPresenters.Push(graphPresenter);
        }

        private GraphAsset m_GraphAsset;

        private SerializedObject m_SO;

        private SerializedProperty m_NodeDataListProperty;

        private NodeSearchEntry m_NodeSearchEntry;

        private CustomGraphView m_GraphView;

        private readonly List<BaseNodePresenter> m_NodePresenters = new();

        public void Initialize(GraphAsset graphAsset)
        {
            m_GraphAsset = graphAsset;
            m_SO = new SerializedObject(graphAsset);
            m_NodeDataListProperty = m_SO.FindProperty("m_NodesData");
            m_NodeSearchEntry = NodeSearchEntry.GetSearchEntry(graphAsset.GraphType);
            m_GraphView = new CustomGraphView(graphAsset)
            {
                nodeCreationRequest = ShowNodeSearchView
            };
            m_GraphView.graphViewChanged += OnGraphViewChanged;
            m_GraphView.OnEdgeConnect += OnEdgeConnect;
            IReadOnlyList<BaseNodeData> nodesData = graphAsset.GetNodesData();
            for (int i = 0; i < nodesData.Count; i++)
            {
                BaseNodeData nodeData = nodesData[i];
                BaseNodePresenter nodePresenter = BaseNodePresenter.AllocateNodePresenter(nodeData.GetType());
                if (nodePresenter != null)
                {
                    nodePresenter.Initialize(nodeData, m_NodeDataListProperty.GetArrayElementAtIndex(i));
                    AddNodePresenter(nodePresenter);
                }
            }

            //TODO:连线的时候有可能存在端口数据被意外删除，比如程序删除了某个节点的端口代码，导致序列化数据不存在这个端口，但是连线数据还在（还在的数据只有连接到删除端口的数据）；
            // 再比如代理辅助节点删除了一个端口代理，代理节点依旧保存了原始的数据。
            // 对此需要一个类或者集合用于保存这些意外删除的数据的连线，因为只能获取到连线数据，端口名、方向（运行时数据）都找不到了。
            for (int i = 0; i < m_NodePresenters.Count; i++)
            {
                BaseNodePresenter fromNodePresenter = m_NodePresenters[i];
                IReadOnlyList<BasePortPresenter> fromPortPresenters = fromNodePresenter.GetPortPresenters();
                for (int j = 0; j < fromPortPresenters.Count; j++)
                {
                    BasePortPresenter fromPortPresenter = fromPortPresenters[j];
                    BasePortData fromPortData = fromPortPresenter.GetPortData();
                    int portConnectionDataCount = fromPortData.GetPortConnectionsDataCount();
                    for (int k = 0; k < portConnectionDataCount; k++)
                    {
                        PortConnectionData portConnectionData = fromPortData.PortConnectionDataOfIndex(k);
                        for (int l = 0; l < m_NodePresenters.Count; l++)
                        {
                            BaseNodePresenter toNodePresenter = m_NodePresenters[l];
                            if (toNodePresenter.GetNodeData().GetNodeID() == portConnectionData.NodeID)
                            {
                                IReadOnlyList<BasePortPresenter> toPortPresenters = toNodePresenter.GetPortPresenters();
                                for (int m = 0; m < toPortPresenters.Count; m++)
                                {
                                    BasePortPresenter toPortPresenter = toPortPresenters[m];
                                    if (toPortPresenter.GetPortData().GetPortID() == portConnectionData.PortID)
                                    {
                                        Edge edge = fromPortPresenter.GetPortView().ConnectTo(toPortPresenter.GetPortView());
                                        m_GraphView.AddElement(edge);
                                        break;
                                    }
                                }
                                break;
                            }
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

        public IReadOnlyList<BaseNodePresenter> GetNodePresenters()
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

        public void AddNodePresenter(BaseNodePresenter nodePresenter)
        {
            m_GraphView.AddNodeView(nodePresenter.GetNodeView());
            m_NodePresenters.Add(nodePresenter);
        }

        public void RemoveNodePresenter(BaseNodePresenter nodePresenter)
        {
            m_GraphView.RemoveNodeView(nodePresenter.GetNodeView());
            m_NodePresenters.Remove(nodePresenter);
        }

        public void UpdateSO()
        {
            m_SO.Update();
        }

        //TODO:在外边调用，把OnRelease里的内容搬到外面去
        public void OnRelease()
        {
            ReleaseGraphPresenter(this);
            for (int i = 0; i < m_NodePresenters.Count; i++)
            {
                m_NodePresenters[i].OnRelease();
            }
        }

        private GraphViewChange OnGraphViewChanged(GraphViewChange changeValue)
        {
            //TODO:需要支持Undo
            if (changeValue.elementsToRemove != null)
            {
                for (int i = 0; i < changeValue.elementsToRemove.Count; i++)
                {
                    if (changeValue.elementsToRemove[i] is NodeView nodeView)
                    {
                        m_GraphAsset.RemoveNodeData(nodeView.BindNodeData);
                        m_GraphView.RemoveNodeView(nodeView);
                    }
                    else if (changeValue.elementsToRemove[i] is Edge edge)
                    {
                        PortView inputPortView = (PortView)edge.input;
                        PortView outputPortView = (PortView)edge.output;
                        inputPortView.BindPortData.Disconnect(outputPortView.BindPortData);
                        outputPortView.BindPortData.Disconnect(inputPortView.BindPortData);
                        CustomGraphView.DisConnect(m_GraphView, edge);
                    }
                }
            }
            if (changeValue.movedElements != null)
            {
                for (int i = 0; i < changeValue.movedElements.Count; i++)
                {
                    if (changeValue.movedElements[i] is NodeView nodeView)
                    {
                        nodeView.BindNodeData.Position += changeValue.moveDelta;
                    }
                }
            }
            return changeValue;
        }

        private void ShowNodeSearchView(NodeCreationContext context)
        {
            SearchWindow.Open(new SearchWindowContext(context.screenMousePosition), m_NodeSearchEntry);
        }

        private void OnEdgeConnect(Edge edge)
        {
            PortView inputPortView = (PortView)edge.input;
            PortView outputPortView = (PortView)edge.output;
            bool isInputCanConnectOutput = inputPortView.BindPortData.CanConnect(outputPortView.BindPortData);
            bool isOutputCanConnectInput = outputPortView.BindPortData.CanConnect(inputPortView.BindPortData);
            if (!(isInputCanConnectOutput ^ isOutputCanConnectInput))
            {
                if (isInputCanConnectOutput)
                {
                    Debug.LogError("This can not know witch one to connect because of both port can connect to other port");
                }
                return;
            }
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
                //区分不了这条连线是输入端口发起的连接还是输出端口发起的连接，所以直接调用两个的Disconnect
                toRemoveInputPortView.BindPortData.Disconnect(toRemoveOutputPortView.BindPortData);
                toRemoveOutputPortView.BindPortData.Disconnect(toRemoveInputPortView.BindPortData);
                //视图上是必须两个端口都调用Disconnect
                CustomGraphView.DisConnect(m_GraphView, singleConnectionToRemove);
            }
            if (isInputCanConnectOutput)
            {
                inputPortView.BindPortData.Connect(outputPortView.BindPortData);
            }
            else
            {
                outputPortView.BindPortData.Connect(inputPortView.BindPortData);
            }
            CustomGraphView.Connect(m_GraphView, edge);
        }
    }
}