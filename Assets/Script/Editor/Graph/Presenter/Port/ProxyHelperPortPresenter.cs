using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using YBFramework.Bridge.Data;

namespace YBFramework.Editor.Graph
{
    [RuntimeToEditor(typeof(ProxyHelperPortData))]
    public sealed class ProxyHelperPortPresenter : BasePortPresenter
    {
        public override void Initialize(BasePortData portData, SerializedProperty portSerializedProperty)
        {
            base.Initialize(portData, portSerializedProperty);
            VisualElement portContentView = new();
            portContentView.Add(m_PortContentView);
            ProxyHelperPortData proxyHelperPortData = (ProxyHelperPortData)portData;
            TextField proxyNameField = new()
            {
                value = proxyHelperPortData.ProxyName
            };
            proxyNameField.RegisterValueChangedCallback(OnProxyNameChange);
            m_PortView.RegisterOnConnectCallback(OnConnect);
            m_PortView.RegisterOnDisconnectCallback(OnDisconnect);
            portContentView.Add(proxyNameField);
            m_PortContentView = portContentView;
        }

        private void OnProxyNameChange(ChangeEvent<string> evt)
        {
            //TODO:需要支持Undo
            ((ProxyHelperPortData)m_PortData).ProxyName = evt.newValue;
        }

        private void OnConnect(Port other)
        {
            PortView otherPortView = (PortView)other;
            ProxyHelperPortData proxyHelperPortData = (ProxyHelperPortData)m_PortData;
            //TODO:需要支持Undo
            proxyHelperPortData.ProxyName = otherPortView.BindPortData.GetPortName();
            proxyHelperPortData.SetTargetPortData(otherPortView.BindPortData);
        }

        private void OnDisconnect(Port other)
        {
            PortView otherPortView = (PortView)other;
            ProxyHelperPortData proxyHelperPortData = (ProxyHelperPortData)m_PortData;
            //TODO:需要支持Undo
            proxyHelperPortData.ProxyName = null;
            proxyHelperPortData.SetTargetPortData(null);
        }
    }
}