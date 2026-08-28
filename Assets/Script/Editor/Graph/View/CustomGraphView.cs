using System;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using YBFramework.Bridge.Data;

namespace YBFramework.Editor.Graph
{
    public sealed class CustomGraphView : GraphView
    {
        /// <summary>
        /// CustomGraphView视图绑定的GraphAsset Data。
        /// 正常MVP架构是不允许数据Data和视图View之间有联系，但是为了用户操作视图时，能够快捷的获取到数据才这么做，不然只有去Presenter中一级一级查找非常耗时。
        /// </summary>
        public readonly GraphAsset BindGraphAsset;

        private readonly List<NodeView> m_NodeViews = new();

        private readonly List<Port> m_CompatiblePorts = new();

        public Action<Edge> OnEdgeConnect;

        public CustomGraphView(GraphAsset graphAsset)
        {
            BindGraphAsset = graphAsset;
            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());

            GridBackground grid = new();
            grid.StretchToParentSize();
            Insert(0, grid);
            this.StretchToParentSize();
        }

        public void AddNodeView(NodeView nodeView)
        {
            m_NodeViews.Add(nodeView);
            AddElement(nodeView);
        }

        public void RemoveNodeView(NodeView nodeView)
        {
            if (m_NodeViews.Remove(nodeView))
            {
                nodeView.ClearPortContentViews();
                RemoveElement(nodeView);
            }
        }

        private NodeView GetNodeView(int nodeID)
        {
            for (int i = 0; i < m_NodeViews.Count; i++)
            {
                if (m_NodeViews[i].NodeDataPresenter.GetNodeData().GetNodeID() == nodeID)
                {
                    return m_NodeViews[i];
                }
            }
            return null;
        }

        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            Direction direction = startPort.direction;
            m_CompatiblePorts.Clear();
            foreach (NodeView nodeView in m_NodeViews)
            {
                IReadOnlyList<PortView> portViews = nodeView.GetPortViews();
                for (int i = 0; i < portViews.Count; i++)
                {
                    PortView portView = portViews[i];
                    if (portView.direction != direction)
                    {
                        m_CompatiblePorts.Add(portView);
                    }
                }
            }
            return m_CompatiblePorts;
        }
    }
}