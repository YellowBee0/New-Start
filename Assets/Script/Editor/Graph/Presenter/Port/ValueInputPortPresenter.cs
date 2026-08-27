using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using YBFramework.Bridge.Data;

namespace YBFramework.Editor.Graph
{
    [RuntimeToEditor(typeof(ValueInputPortData<>))]
    public sealed class ValueInputPortPresenter : BasePortPresenter
    {
        private PropertyField m_ValueField;

        private BasePortData m_PortData;

        private PortView m_PortView;

        private VisualElement m_PortContentView;

        public override void Initialize(BasePortData portData, SerializedProperty portSerializedProperty)
        {
            m_PortData = portData;
            m_PortView = new PortView(portData, portData.GetPortName(), portData.GetDirection(), portData.GetCapacity(), portData.GetPortColor());
            m_PortContentView = m_PortView;
            SerializedProperty valueProperty = portSerializedProperty.FindPropertyRelative("Value");
            if (valueProperty != null)
            {
                VisualElement portContentView = new();
                m_PortView.RegisterOnConnectCallback(OnConnectOther);
                m_PortView.RegisterOnDisconnectCallback(OnDisconnectOther);
                m_ValueField = new PropertyField();
                m_ValueField.styleSheets.Add(StyleSheetManager.LoadStylesheet("GraphViewLabel"));
                m_ValueField.BindProperty(valueProperty);
                portContentView.Add(m_PortContentView);
                portContentView.Add(m_ValueField);
                m_PortContentView = portContentView;
            }
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

        public override void OnRelease()
        {
            base.OnRelease();
            m_PortView.UnregisterOnConnectCallback(OnConnectOther);
            m_PortView.UnregisterOnDisconnectCallback(OnDisconnectOther);
        }

        private void OnConnectOther(Port other)
        {
            m_ValueField.enabledSelf = false;
        }

        private void OnDisconnectOther(Port other)
        {
            m_ValueField.enabledSelf = true;
        }
    }
}