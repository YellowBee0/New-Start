#if UNITY_EDITOR
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using YBFramework.Bridge;

namespace YBFramework.EditorOnly
{
    public sealed class PortView : Port
    {
        private readonly BasePortData m_Port;

        public PortView(BasePortData port) : base(Orientation.Horizontal, port.GetPortViewArgs().Direction, port.GetPortViewArgs().Capacity, null)
        {
            m_Port = port;
        }

        public BasePortData GetPort()
        {
            return m_Port;
        }

        private sealed class EdgeConnectorListener : IEdgeConnectorListener
        {
            public void OnDropOutsidePort(Edge edge, Vector2 position)
            {
            }

            public void OnDrop(GraphView graphView, Edge edge)
            {
            }
        }
    }
}
#endif