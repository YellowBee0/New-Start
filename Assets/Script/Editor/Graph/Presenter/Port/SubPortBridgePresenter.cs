using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using YBFramework.Bridge.Data;

namespace YBFramework.Editor.Graph
{
    [RuntimeToEditor(typeof(ExposePortData))]
    public sealed class SubPortBridgePresenter : BasePortPresenter
    {
        private ExposePortData m_PortData;

        private PortView m_PortView;

        private VisualElement m_PortContentView;

        private void OnProxyNameChange(ChangeEvent<string> evt)
        {
            //TODO:需要支持Undo
            m_PortData.ExposePortDisplayName = evt.newValue;
        }

        private void OnConnect(Port other)
        {
            PortView otherPortView = (PortView)other;
            //TODO:需要支持Undo
            m_PortData.ExposePortDisplayName = otherPortView.BindPortData.GetPortName();
        }

        private void OnDisconnect(Port other)
        {
            //TODO:需要支持Undo
            m_PortData.ExposePortDisplayName = null;
        }

        public override void Initialize(BasePortData portData, SerializedProperty portSerializedProperty)
        {
            m_PortData = (ExposePortData)portData;
            m_PortView = new PortView(portData, portData.GetPortName(), portData.GetDirection(), portData.GetCapacity(), portData.GetPortColor());
            VisualElement portContentView = new();
            portContentView.Add(m_PortView);
            TextField proxyNameField = new()
            {
                value = m_PortData.ExposePortDisplayName
            };
            proxyNameField.RegisterValueChangedCallback(OnProxyNameChange);
            m_PortView.RegisterOnConnectCallback(OnConnect);
            m_PortView.RegisterOnDisconnectCallback(OnDisconnect);
            portContentView.Add(proxyNameField);
            m_PortContentView = portContentView;
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