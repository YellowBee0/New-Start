using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace YBFramework.Editor.Graph
{
    public sealed class EdgeConnectorListener : IEdgeConnectorListener
    {
        public void OnDropOutsidePort(Edge edge, Vector2 position)
        {
        }

        public void OnDrop(GraphView graphView, Edge edge)
        {
            CustomGraphView customGraphView = (CustomGraphView)graphView;
            customGraphView.OnEdgeConnect?.Invoke(edge);
        }
    }
}