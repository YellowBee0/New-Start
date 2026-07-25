using UnityEditor;
using UnityEngine.UIElements;
using YBFramework.Bridge.Data;

namespace YBFramework.Editor
{
    [GraphDrawer(typeof(ProxyPortData))]
    public sealed class ProxyPortDrawer : BasePortDrawer
    {
        public override VisualElement CreatePortContentView(BasePortData portData, SerializedProperty serializedProperty, out PortView portView)
        {
            ProxyPortData proxyPortData = (ProxyPortData)portData;
            BasePortDrawer portDrawer = AllocatePortDrawer(proxyPortData.ProxyTargetClonedPortData.GetType());
            if (portDrawer != null)
            {
                return portDrawer.CreatePortContentView(proxyPortData.ProxyTargetClonedPortData, serializedProperty.FindPropertyRelative("ProxyTargetClonedPortData"), out portView);
            }
            portView = null;
            return null;
        }
    }
}