#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using YBFramework.Bridge.Data;

namespace YBFramework.Bridge.Editor
{
    public sealed class CustomGraphView : GraphView
    {
        public readonly GraphAsset BindGraphAsset;

        private readonly NodeSearchEntry m_NodeSearchEntry;

        private readonly List<NodeView> m_NodeViews = new();

        private readonly List<Port> m_CompatiblePorts = new();

        public CustomGraphView(GraphAsset bindGraphAsset, NodeSearchEntry nodeSearchEntry)
        {
            BindGraphAsset = bindGraphAsset;
            m_NodeSearchEntry = nodeSearchEntry;
            nodeCreationRequest = ShowNodeSearchView;
            graphViewChanged += OnGraphViewChanged;
            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());

            GridBackground grid = new();
            grid.StretchToParentSize();
            Insert(0, grid);
            this.StretchToParentSize();
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
                        BindGraphAsset.RemoveNodeData(nodeView.BindNodeData);
                        RemoveNodeView(nodeView);
                    }
                    else if (changeValue.elementsToRemove[i] is Edge edge)
                    {
                        PortView inputPortView = (PortView)edge.input;
                        PortView outputPortView = (PortView)edge.output;
                        inputPortView.BindPortData.Disconnect(outputPortView.BindPortData);
                        outputPortView.BindPortData.Disconnect(inputPortView.BindPortData);
                        edge.input.Disconnect(edge);
                        edge.output.Disconnect(edge);
                        Remove(edge);
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
                if (m_NodeViews[i].BindNodeData.NodeID == nodeID)
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
    }
}
#endif