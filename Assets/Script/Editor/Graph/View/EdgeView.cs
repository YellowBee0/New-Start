using UnityEditor.Experimental.GraphView;

namespace YBFramework.Editor.Graph
{
    public sealed class EdgeView : Edge
    {
        private PortView m_FromPortView;

        private PortView m_ToPortView;

        public void SetConnectDirection(PortView fromPortView, PortView toPortView)
        {
            m_FromPortView = fromPortView;
            m_ToPortView = toPortView;
        }

        public PortView GetFromPortView()
        {
            return m_FromPortView;
        }

        public PortView GetToPortView()
        {
            return m_ToPortView;
        }
    }
}