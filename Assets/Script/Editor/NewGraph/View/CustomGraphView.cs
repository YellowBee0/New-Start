using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

namespace YBFramework.Editor.NewGraph
{
    public sealed class CustomGraphView : GraphView
    {
        /// <summary>
        /// 只有缓存Drawer，不然每次视图上用户操作了，不能方便地获取到操作的数据
        /// </summary>
        private GraphAssetDrawer m_GraphAssetDrawer;

        private readonly List<NodeView> m_NodeViews = new();

        private readonly List<Port> m_CompatiblePorts = new();

        public CustomGraphView()
        {
            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());
            GridBackground grid = new();
            grid.StretchToParentSize();
            Insert(0, grid);
            this.StretchToParentSize();
        }

        public GraphAssetDrawer GetGraphAssetDrawer()
        {
            return m_GraphAssetDrawer;
        }

        public IReadOnlyList<NodeView> GetNodeViews()
        {
            return m_NodeViews;
        }
        
        public void AddNodeView(NodeView nodeView)
        {
            if (!m_NodeViews.Contains(nodeView))
            {
                m_NodeViews.Add(nodeView);
                AddElement(nodeView);
            }
        }

        public void RemoveNodeView(NodeView nodeView)
        {
            if (m_NodeViews.Remove(nodeView))
            {
                nodeView.ClearPortViews();
                RemoveElement(nodeView);
                NodeView.Release(nodeView);
            }
        }

        public NodeView FindNodeView(int nodeID)
        {
            for (int i = 0; i < m_NodeViews.Count; i++)
            {
                NodeView nodeView = m_NodeViews[i];
                if (nodeView.GetNodeID() == nodeID)
                {
                    return nodeView;
                }
            }
            return null;
        }

        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            Direction direction = startPort.direction;
            m_CompatiblePorts.Clear();
            for (int i = 0; i < m_NodeViews.Count; i++)
            {
                IReadOnlyList<PortView> portViews = m_NodeViews[i].GetPortViews();
                for (int j = 0; j < portViews.Count; j++)
                {
                    PortView portView = portViews[j];
                    if (portView.direction != direction)
                    {
                        m_CompatiblePorts.Add(portView);
                    }
                }
            }
            return m_CompatiblePorts;
        }

        public static void Connect(PortView fromPortView, PortView toPortView, GraphView graphView)
        {
            EdgeView edgeView = fromPortView.ConnectTo<EdgeView>(toPortView);
            edgeView.SetConnectDirection(fromPortView, toPortView);
            graphView.AddElement(edgeView);
        }

        public static void Connect(PortView fromPortView, PortView toPortView, EdgeView edgeView, GraphView graphView)
        {
            fromPortView.Connect(edgeView);
            toPortView.Connect(edgeView);
            edgeView.SetConnectDirection(fromPortView, toPortView);
            graphView.AddElement(edgeView);
        }

        public static void Disconnect(PortView fromPortView, PortView toPortView, GraphView graphView)
        {
            Edge connection = fromPortView.FindConnection(toPortView);
            if (connection != null)
            {
                fromPortView.Disconnect(connection);
                toPortView.Disconnect(connection);
                graphView.RemoveElement(connection);
            }
        }

        public static void Disconnect(EdgeView edgeView, GraphView graphView)
        {
            edgeView.input.Disconnect(edgeView);
            edgeView.output.Disconnect(edgeView);
            graphView.RemoveElement(edgeView);
        }
        
        #region Pool

        private static readonly Stack<CustomGraphView> s_Pool = new();

        public static CustomGraphView Allocate(GraphAssetDrawer graphAssetDrawer)
        {
            CustomGraphView customGraphView = s_Pool.Count > 0 ? s_Pool.Pop() : new CustomGraphView();
            customGraphView.m_NodeViews.Clear();
            customGraphView.m_GraphAssetDrawer = graphAssetDrawer;
            return customGraphView;
        }

        public static void Release(CustomGraphView customGraphView)
        {
            s_Pool.Push(customGraphView);
        }

        #endregion
    }
}