using UnityEditor;
using UnityEngine.UIElements;
using YBFramework.Bridge.Data;

namespace YBFramework.Editor.Graph
{
    [EditorPresenter(typeof(ProxyHelperPortData))]
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
            portContentView.Add(proxyNameField);
            m_PortContentView = portContentView;
        }

        private void OnProxyNameChange(ChangeEvent<string> evt)
        {
            //TODO:需要支持Undo
            ((ProxyHelperPortData)m_PortData).ProxyName = evt.newValue;
        }
    }
}