using UnityEditor;
using UnityEngine.UIElements;
using YBFramework.Bridge.Data;
using YBFramework.Bridge.Editor;

namespace YBFramework.Editor
{
    [GraphDrawer(typeof(ProxyTargetPortData))]
    public sealed class ProxyTargetPortDrawer : BasePortDrawer
    {
        private ProxyTargetPortData m_DrawPortData;

        public override VisualElement CreatePortContentView(BasePortData portData, SerializedProperty serializedProperty, out PortView portView)
        {
            m_DrawPortData = (ProxyTargetPortData)portData;
            VisualElement portContentView = base.CreatePortContentView(portData, serializedProperty, out portView);
            TextField proxyNameField = new()
            {
                value = m_DrawPortData.ProxyName
            };
            proxyNameField.RegisterValueChangedCallback(OnProxyNameChange);
            portContentView.Add(proxyNameField);
            return portContentView;
        }

        private void OnProxyNameChange(ChangeEvent<string> evt)
        {
            //TODO:需要支持Undo
            m_DrawPortData.ProxyName = evt.newValue;
        }
    }
}