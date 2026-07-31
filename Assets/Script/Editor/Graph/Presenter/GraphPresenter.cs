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
            m_GraphAsset.Initialize();
            m_SO = new SerializedObject(graphAsset);
            m_SO.Update();
            m_NodeDataListProperty = m_SO.FindProperty("m_NodesData");
            m_NodeSearchEntry = NodeSearchEntry.GetSearchEntry(graphAsset.GetGraphType());
            m_GraphView = new CustomGraphView(graphAsset)
            {
                nodeCreationRequest = ShowNodeSearchView,
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
                        edge.input.Disconnect(edge);
                        edge.output.Disconnect(edge);
                        m_GraphView.RemoveElement(edge);
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
                toRemoveInputPortView.Disconnect(singleConnectionToRemove);
                toRemoveOutputPortView.Disconnect(singleConnectionToRemove);
                m_GraphView.RemoveElement(singleConnectionToRemove);
            }
            if (isInputCanConnectOutput)
            {
                inputPortView.BindPortData.Connect(outputPortView.BindPortData);
            }
            else
            {
                outputPortView.BindPortData.Connect(inputPortView.BindPortData);
            }
            inputPortView.Connect(edge);
            outputPortView.Connect(edge);
            m_GraphView.AddElement(edge);
        }
    }
}