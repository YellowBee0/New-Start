using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace YBFramework.Editor.NewGraph
{
    public sealed class NodeView : Node
    {
        /// <summary>
        /// 只有缓存Drawer，不然每次视图上用户操作了，不能方便地获取到操作的数据
        /// </summary>
        private BaseNodeDrawer m_NodeDrawer;

        private readonly List<PortView> m_PortViews = new();

        public BaseNodeDrawer GetNodeDrawer()
        {
            return m_NodeDrawer;
        }

        public IReadOnlyList<PortView> GetPortViews()
        {
            return m_PortViews;
        }

        public void AddPortView(PortView portView)
        {
            VisualElement portContentView = portView.GetPortDrawer().GetPortContentView();
            portContentView.style.borderBottomColor = Color.black;
            portContentView.style.borderBottomWidth = .2f;
            if (portView.direction == Direction.Input)
            {
                inputContainer.Add(portContentView);
            }
            else
            {
                outputContainer.Add(portContentView);
            }
            m_PortViews.Add(portView);
        }

        public void RemovePortView(PortView portView)
        {
            if (m_PortViews.Remove(portView))
            {
                VisualElement portContentView = portView.GetPortDrawer().GetPortContentView();
                if (portView.direction == Direction.Input)
                {
                    inputContainer.Remove(portContentView);
                }
                else
                {
                    outputContainer.Remove(portContentView);
                }
                portView.ClearConnections();
                PortView.Release(portView);
            }
        }

        public void ClearPortViews()
        {
            for (int i = 0; i < m_PortViews.Count; i++)
            {
                PortView portView = m_PortViews[i];
                portView.ClearConnections();
                PortView.Release(portView);
            }
            inputContainer.Clear();
            outputContainer.Clear();
            m_PortViews.Clear();
        }

        public void RefreshPortContainerDisplay()
        {
            inputContainer.style.display = inputContainer.childCount == 0 ? DisplayStyle.None : DisplayStyle.Flex;
            outputContainer.style.display = outputContainer.childCount == 0 ? DisplayStyle.None : DisplayStyle.Flex;
        }
        
        #region Pool

        private static readonly Stack<NodeView> s_Pool = new();

        public static NodeView Allocate(BaseNodeDrawer nodeDrawer, string nodeName, Vector2 position)
        {
            NodeView nodeView = s_Pool.Count > 0 ? s_Pool.Pop() : new NodeView();
            nodeView.m_NodeDrawer = nodeDrawer;
            nodeView.title = nodeName;
            nodeView.SetPosition(new Rect(position, Vector2.zero));
            nodeView.m_PortViews.Clear();
            return nodeView;
        }

        public static void Release(NodeView nodeView)
        {
            s_Pool.Push(nodeView);
        }

        #endregion
    }
}