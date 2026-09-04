using UnityEngine.UIElements;
using YBFramework.Bridge.Data;

namespace YBFramework.Editor.Graph
{
    [RuntimeToEditor(typeof(ExposePortData))]
    public sealed class ExposePortDrawer : BasePortDrawer
    {
        private ExposePortData m_ExposePortData;

        private PortView m_PortView;

        private VisualElement m_PortContentView;

        private readonly TextField m_DisplayNameTextField;

        public ExposePortDrawer()
        {
            m_DisplayNameTextField = new TextField();
            m_DisplayNameTextField.RegisterValueChangedCallback(OnDisplayNameChange);
        }

        private void OnDisplayNameChange(ChangeEvent<string> evt)
        {
            //TODO:需要支持Undo
            m_ExposePortData.ExposePortDisplayName = evt.newValue;
        }

        public override BasePortData GetPortData()
        {
            return m_ExposePortData;
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
            m_ExposePortData = (ExposePortData)portData;
            m_PortView = PortView.Allocate(portData.GetDirection(), portData.GetCapacity(), portData.GetPortID(), this, portData.GetPortName(), portData.GetPortColor());
            VisualElement portContentView = new();
            portContentView.Add(m_PortView);
            m_DisplayNameTextField.SetValueWithoutNotify(m_ExposePortData.ExposePortDisplayName);
            portContentView.Add(m_DisplayNameTextField);
            m_PortContentView = portContentView;
            return m_PortView;
        }

        public override void OnRelease()
        {
            m_PortContentView.Remove(m_DisplayNameTextField);
        }
    }
}