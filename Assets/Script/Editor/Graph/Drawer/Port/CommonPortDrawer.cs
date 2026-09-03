using UnityEngine.UIElements;
using YBFramework.Bridge.Data;

namespace YBFramework.Editor.Graph
{
    [RuntimeToEditor(typeof(BasePortData))]
    public sealed class CommonPortDrawer : BasePortDrawer
    {
        private BasePortData m_PortData;

        private VisualElement m_PortContentView;

        private PortView m_PortView;

        public override BasePortData GetPortData()
        {
            return m_PortData;
        }

        public override VisualElement GetPortContentView()
        {
            return m_PortContentView;
        }

        public override PortView GetPortView()
        {
            return m_PortView;
        }

        protected override PortView OnDrawPortView(BasePortData portData)
        {
            m_PortData = portData;
            m_PortView = PortView.Allocate(portData.GetDirection(), portData.GetCapacity(), portData.GetPortID(), this, portData.GetPortName(), portData.GetPortColor());
            m_PortContentView = m_PortView;
            return m_PortView;
        }
    }
}