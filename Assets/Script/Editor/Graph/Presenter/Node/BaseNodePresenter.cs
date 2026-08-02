using System;
using System.Collections.Generic;
using UnityEditor;
using YBFramework.Bridge.Data;
using YBFramework.Common;

namespace YBFramework.Editor.Graph
{
    [RuntimeToEditor(typeof(BaseNodeData))]
    public class BaseNodePresenter
    {
        private static readonly Dictionary<Type, Stack<BaseNodePresenter>> s_NodePresenters = new();

        public static BaseNodePresenter AllocateNodePresenter(Type nodeDataType)
        {
            Type nodePresenterType = RuntimeToEditorMap.GetInstance().GetDrawerType(nodeDataType);
            if (nodePresenterType == null)
            {
                return null;
            }
            if (!s_NodePresenters.TryGetValue(nodePresenterType, out Stack<BaseNodePresenter> nodePresenters))
            {
                nodePresenters = new Stack<BaseNodePresenter>();
                s_NodePresenters.Add(nodePresenterType, nodePresenters);
            }
            return nodePresenters.Count > 0 ? nodePresenters.Pop() : Activator.CreateInstance(nodePresenterType) as BaseNodePresenter;
        }

        public static void ReleaseNodePresenter(BaseNodePresenter nodePresenter)
        {
            Type nodePresenterType = nodePresenter.GetType();
            if (s_NodePresenters.TryGetValue(nodePresenterType, out Stack<BaseNodePresenter> nodePresenters))
            {
                nodePresenters.Push(nodePresenter);
            }
        }

        protected BaseNodeData m_NodeData;

        protected NodeView m_NodeView;

        protected readonly List<BasePortPresenter> m_PortPresenters = new();

        public virtual void Initialize(BaseNodeData nodeData, SerializedProperty nodeSerializedProperty)
        {
            m_NodeData = nodeData;
            m_NodeView = new NodeView(nodeData);
            foreach (BasePortData portData in (IValueIterator<BasePortData>)nodeData)
            {
                BasePortPresenter portPresenter = BasePortPresenter.AllocatePortPresenter(portData.GetType());
                if (portPresenter != null)
                {
                    portPresenter.Initialize(portData, nodeSerializedProperty.FindPropertyRelative(portData.GetFiledName()));
                    AddPortPresenter(portPresenter);
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

        public IReadOnlyList<BasePortPresenter> GetPortPresenters()
        {
            return m_PortPresenters;
        }

        public void AddPortPresenter(BasePortPresenter portPresenter)
        {
            m_NodeView.AddPortView(portPresenter.GetPortView());
            m_NodeView.AddPortContentView(portPresenter.GetPortContentView(), portPresenter.GetPortData().GetDirection());
            m_PortPresenters.Add(portPresenter);
        }

        public void RemovePortPresenter(BasePortPresenter portPresenter)
        {
            m_NodeView.RemovePortView(portPresenter.GetPortView());
            m_NodeView.RemovePortContentView(portPresenter.GetPortContentView(), portPresenter.GetPortData().GetDirection());
            m_PortPresenters.Add(portPresenter);
        }

        public void OnRelease()
        {
            ReleaseNodePresenter(this);
            for (int i = 0; i < m_PortPresenters.Count; i++)
            {
                m_PortPresenters[i].OnRelease();
            }
            m_PortPresenters.Clear();
        }
    }
}