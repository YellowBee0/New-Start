using UnityEditor;
using UnityEngine.UIElements;
using YBFramework.Bridge.Data;

namespace YBFramework.Editor.Graph
{
    [RuntimeToEditor(typeof(BasePortData))]
    public sealed class CommonPortDataPresenter : BasePortDataPresenter
    {
        private BasePortData m_PortData;

        private PortView m_PortView;

        private VisualElement m_PortContentView;

        public override void Initialize(BaseNodeDataPresenter nodeDataPresenter, BasePortData portData, SerializedProperty portSerializedProperty)
        {
            base.Initialize(nodeDataPresenter, portData, portSerializedProperty);
            m_PortData = portData;
            m_PortView = new PortView(this, portData.GetPortName(), portData.GetDirection(), portData.GetCapacity(), portData.GetPortColor());
            m_PortContentView = m_PortView;
        }

        public override BasePortData GetPortData()
        {
            return m_PortData;
        }

        public override PortView GetPortView()
        {
            return m_PortView;
        }

        public override VisualElement GetPortContentView()
        {
            return m_PortContentView;
        }
    }
}