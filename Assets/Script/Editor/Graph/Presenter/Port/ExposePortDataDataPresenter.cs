using UnityEditor;
using UnityEngine.UIElements;
using YBFramework.Bridge.Data;

namespace YBFramework.Editor.Graph
{
    [RuntimeToEditor(typeof(ExposePortData))]
    public sealed class ExposePortDataDataPresenter : BasePortDataPresenter
    {
        private ExposePortData m_PortData;

        private PortView m_PortView;

        private VisualElement m_PortContentView;

        private TextField m_DisplayNameTextField;

        private void OnDisplayNameChange(ChangeEvent<string> evt)
        {
            //TODO:需要支持Undo
            m_PortData.ExposePortDisplayName = evt.newValue;
        }

        public override void Initialize(BasePortData portData, SerializedProperty portSerializedProperty)
        {
            m_PortData = (ExposePortData)portData;
            m_PortView = new PortView(this, portData.GetPortName(), portData.GetDirection(), portData.GetCapacity(), portData.GetPortColor());
            VisualElement portContentView = new();
            portContentView.Add(m_PortView);
            m_DisplayNameTextField = new TextField
            {
                value = m_PortData.ExposePortDisplayName
            };
            m_DisplayNameTextField.RegisterValueChangedCallback(OnDisplayNameChange);
            portContentView.Add(m_DisplayNameTextField);
            m_PortContentView = portContentView;
        }

        public override void OnConnect(BasePortDataPresenter other)
        {
            //TODO:需要支持Undo
            string displayName = other.GetPortData().GetPortName();
            m_DisplayNameTextField.SetValueWithoutNotify(displayName);
            m_PortData.ExposePortDisplayName = displayName;
        }

        public override void OnDisconnect(BasePortDataPresenter other)
        {
            m_DisplayNameTextField.SetValueWithoutNotify(null);
            m_PortData.ExposePortDisplayName = null;
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