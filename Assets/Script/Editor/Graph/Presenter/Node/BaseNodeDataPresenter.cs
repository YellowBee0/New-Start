using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using YBFramework.Bridge.Data;

namespace YBFramework.Editor.Graph
{
    [RuntimeToEditor(typeof(BaseNodeData))]
    public class BaseNodeDataPresenter
    {
        private static readonly List<Edge> s_RemoveEdgesCache = new();

        private static readonly Dictionary<Type, Stack<BaseNodeDataPresenter>> s_NodePresenters = new();

        public static BaseNodeDataPresenter AllocateNodePresenter(Type nodeDataType)
        {
            Type nodePresenterType = RuntimeToEditorMap.GetInstance().GetDrawerType(nodeDataType);
            if (nodePresenterType == null)
            {
                return null;
            }
            if (!s_NodePresenters.TryGetValue(nodePresenterType, out Stack<BaseNodeDataPresenter> nodePresenters))
            {
                nodePresenters = new Stack<BaseNodeDataPresenter>();
                s_NodePresenters.Add(nodePresenterType, nodePresenters);
            }
            return nodePresenters.Count > 0 ? nodePresenters.Pop() : Activator.CreateInstance(nodePresenterType) as BaseNodeDataPresenter;
        }

        public static void ReleaseNodePresenter(BaseNodeDataPresenter nodeDataPresenter)
        {
            if (s_NodePresenters.TryGetValue(nodeDataPresenter.GetType(), out Stack<BaseNodeDataPresenter> nodePresenters))
            {
                nodeDataPresenter.OnRelease();
                nodePresenters.Push(nodeDataPresenter);
            }
        }

        protected BaseNodeData m_NodeData;

        protected NodeView m_NodeView;

        protected GraphAssetPresenter m_GraphAssetPresenter;

        protected readonly List<BasePortDataPresenter> m_PortDataPresenters = new();

        public virtual void Initialize(GraphAssetPresenter graphAssetPresenter, BaseNodeData nodeData, SerializedProperty nodeSerializedProperty)
        {
            m_NodeData = nodeData;
            m_NodeView = new NodeView(this);
            m_GraphAssetPresenter = graphAssetPresenter;
            nodeData.InitializePortData();
            int portDataCount = nodeData.GetPortsDataCount();
            for (int i = 0; i < portDataCount; i++)
            {
                BasePortData portData = nodeData.PortDataOfIndex(i);
                BasePortDataPresenter portDataPresenter = BasePortDataPresenter.AllocatePortPresenter(portData.GetType());
                if (portDataPresenter != null)
                {
                    portDataPresenter.Initialize(this, portData, nodeSerializedProperty.FindPropertyRelative(portData.GetFieldName()));
                    AddPortDataPresenter(portDataPresenter);
                }
            }
            m_NodeView.RefreshPortContainerDisplay();
        }

        public BaseNodeData GetNodeData()
        {
            return m_NodeData;
        }

        public NodeView GetNodeView()
        {
            return m_NodeView;
        }

        public GraphAssetPresenter GetGraphAssetPresenter()
        {
            return m_GraphAssetPresenter;
        }

        public IReadOnlyList<BasePortDataPresenter> GetPortPresenters()
        {
            return m_PortDataPresenters;
        }

        public BasePortDataPresenter FindPortDataPresenter(int portID)
        {
            for (int i = 0; i < m_PortDataPresenters.Count; i++)
            {
                if (m_PortDataPresenters[i].GetPortData().GetPortID() == portID)
                {
                    return m_PortDataPresenters[i];
                }
            }
            return null;
        }
        
        public void AddPortDataPresenter(BasePortDataPresenter portDataPresenter)
        {
            m_NodeView.AddPortContentView(portDataPresenter.GetPortContentView(), portDataPresenter.GetPortView().direction);
            m_PortDataPresenters.Add(portDataPresenter);
        }

        public void RemovePortDataPresenter(BasePortDataPresenter portDataPresenter)
        {
            if (m_PortDataPresenters.Remove(portDataPresenter))
            {
                PortView portView = portDataPresenter.GetPortView();
                m_NodeView.RemovePortContentView(portDataPresenter.GetPortContentView(), portView.direction);
                s_RemoveEdgesCache.Clear();
                s_RemoveEdgesCache.AddRange(portView.connections);
                foreach (Edge connection in s_RemoveEdgesCache)
                {
                    connection.input.Disconnect(connection);
                    connection.output.Disconnect(connection);
                    m_GraphAssetPresenter.GetGraphView().RemoveElement(connection);
                }
                portDataPresenter.GetPortData().DisconnectAll();
                BasePortDataPresenter.ReleasePortPresenter(portDataPresenter);
            }
        }

        public void ClearPortDataPresenter()
        {
            for (int i = m_PortDataPresenters.Count - 1; i >= 0; i--)
            {
                RemovePortDataPresenter(m_PortDataPresenters[i]);
            }
        }

        private void OnRelease()
        {
            for (int i = 0; i < m_PortDataPresenters.Count; i++)
            {
                BasePortDataPresenter.ReleasePortPresenter(m_PortDataPresenters[i]);
            }
            m_PortDataPresenters.Clear();
        }
    }
}