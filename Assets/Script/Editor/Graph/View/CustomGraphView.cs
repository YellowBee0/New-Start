using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

namespace YBFramework.Editor.Graph
{
    public sealed class CustomGraphView : GraphView
    {
        public readonly GraphAssetPresenter GraphAssetPresenter;

        private readonly List<Port> m_CompatiblePorts = new();

        public CustomGraphView(GraphAssetPresenter graphAssetPresenter)
        {
            GraphAssetPresenter = graphAssetPresenter;
            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());
            GridBackground grid = new();
            grid.StretchToParentSize();
            Insert(0, grid);
            this.StretchToParentSize();
        }

        public void OnEdgeConnect(Edge edge)
        {
            GraphAssetPresenter.OnEdgeConnect(edge);
        }

        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            Direction direction = startPort.direction;
            m_CompatiblePorts.Clear();
            IReadOnlyList<BaseNodeDataPresenter> nodeDataPresenters = GraphAssetPresenter.GetNodeDataPresenters();
            for (int i = 0; i < nodeDataPresenters.Count; i++)
            {
                IReadOnlyList<BasePortDataPresenter> portDataPresenters = nodeDataPresenters[i].GetPortPresenters();
                for (int j = 0; j < portDataPresenters.Count; j++)
                {
                    PortView portView = portDataPresenters[j].GetPortView();
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