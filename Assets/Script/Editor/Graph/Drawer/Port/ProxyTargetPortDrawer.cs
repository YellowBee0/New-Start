using UnityEditor;
using UnityEngine.UIElements;
using YBFramework.Bridge.Data;

namespace YBFramework.Editor
{
    [GraphDrawer(typeof(ProxyTargetPortData))]
    public sealed class ProxyTargetPortDrawer : BasePortDrawer
    {
        public override VisualElement CreatePortContentView(BasePortData portData, SerializedProperty serializedProperty, out PortView portView)
        {
            VisualElement portContentView = new();
            portContentView.Add(base.CreatePortContentView(portData, serializedProperty, out portView));
            ProxyTargetPortData proxyTargetPortData = (ProxyTargetPortData)portData;
            TextField proxyNameField = new()
            {
                value = proxyTargetPortData.ProxyName
            };
            proxyNameField.RegisterValueChangedCallback(OnProxyNameChange);
            portContentView.Add(proxyNameField);
            return portContentView;
        }

        private void OnProxyNameChange(ChangeEvent<string> evt)
        {
            //TODO:需要支持Undo
            ((ProxyTargetPortData)m_BindPortData).ProxyName = evt.newValue;
        }
    }
}