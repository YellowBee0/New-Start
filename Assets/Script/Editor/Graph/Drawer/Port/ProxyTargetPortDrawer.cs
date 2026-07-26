using UnityEditor;
using UnityEngine.UIElements;
using YBFramework.Bridge.Data;

namespace YBFramework.Editor
{
    [GraphDrawer(typeof(ProxyHelperPortData))]
    public sealed class ProxyTargetPortDrawer : BasePortDrawer
    {
        public override VisualElement CreatePortContentView(BasePortData portData, SerializedProperty serializedProperty, out PortView portView)
        {
            VisualElement portContentView = new();
            portContentView.Add(base.CreatePortContentView(portData, serializedProperty, out portView));
            ProxyHelperPortData proxyHelperPortData = (ProxyHelperPortData)portData;
            TextField proxyNameField = new()
            {
                value = proxyHelperPortData.ProxyName
            };
            proxyNameField.RegisterValueChangedCallback(OnProxyNameChange);
            portContentView.Add(proxyNameField);
            return portContentView;
        }

        private void OnProxyNameChange(ChangeEvent<string> evt)
        {
            //TODO:需要支持Undo
            ((ProxyHelperPortData)m_BindPortData).ProxyName = evt.newValue;
        }
    }
}