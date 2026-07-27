using UnityEditor;
using UnityEngine.UIElements;
using YBFramework.Bridge.Data;

namespace YBFramework.Editor.Graph
{
    [GraphDrawer(typeof(ProxyPortData))]
    public sealed class ProxyPortDrawer : BasePortDrawer
    {
        public override VisualElement CreatePortContentView(BasePortData portData, SerializedProperty serializedProperty, out PortView portView)
        {
            ProxyPortData proxyPortData = (ProxyPortData)portData;
            BasePortDrawer portDrawer = AllocatePortDrawer(proxyPortData.ClonedTargetPortData.GetType());
            if (portDrawer != null)
            {
                return portDrawer.CreatePortContentView(
                    proxyPortData.ClonedTargetPortData,
                    serializedProperty.FindPropertyRelative(proxyPortData.ClonedTargetPortData.GetFiledName()),
                    out portView);
            }
            portView = null;
            return null;
        }
    }
}