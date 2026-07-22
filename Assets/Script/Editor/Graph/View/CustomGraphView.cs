using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

namespace YBFramework.Editor
{
    public sealed class CustomGraphView : GraphView
    {
        public readonly GraphDrawer BindGraphDrawer;

        private readonly List<NodeView> m_NodeViews = new();

        private readonly List<Port> m_CompatiblePorts = new();

        public CustomGraphView(GraphDrawer bindGraphDrawer)
        {
            BindGraphDrawer = bindGraphDrawer;
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
                RemoveElement(nodeView);
            }
        }

        private NodeView GetNodeView(int nodeID)
        {
            for (int i = 0; i < m_NodeViews.Count; i++)
            {
                if (m_NodeViews[i].BindNodeDrawer.GetBindNodeData().NodeID == nodeID)
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
                    if (portView.direction == direction)
                    {
                        m_CompatiblePorts.Add(portView);
                    }
                }
            }
            return m_CompatiblePorts;
        }

        public void OnRelease()
        {
            GraphWindow.GetInstance().ReleaseGraphDrawer(BindGraphDrawer);
            for (int i = 0; i < m_NodeViews.Count; i++)
            {
                m_NodeViews[i].OnRelease();
            }
        }
    }
}