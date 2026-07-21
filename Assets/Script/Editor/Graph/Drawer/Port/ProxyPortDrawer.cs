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
            //TODO:获取端口绘制器
            BasePortDrawer portDrawer = GraphDrawerMap.GetInstance().GetPortDrawer(proxyPortData.m_ProxyPortData.GetType());
            if (portDrawer != null)
            {
                return portDrawer!.CreatePortContentView(proxyPortData.m_ProxyPortData, serializedProperty.FindPropertyRelative("m_ProxyPortData"), out portView);
            }
            portView = null;
            return null;
        }
    }
}