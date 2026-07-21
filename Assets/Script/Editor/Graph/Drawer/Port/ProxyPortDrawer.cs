using System;
using UnityEditor;
using UnityEngine.UIElements;
using YBFramework.Bridge.Data;
using YBFramework.Bridge.Editor;

namespace YBFramework.Editor
{
    [GraphDrawer(typeof(ProxyPortData))]
    public sealed class ProxyPortDrawer : BasePortDrawer
    {
        public override VisualElement CreatePortContentView(BasePortData portData, SerializedProperty serializedProperty, out PortView portView)
        {
            ProxyPortData proxyPortData = (ProxyPortData)portData;
            //TODO:获取端口绘制器
            Type drawerType = GraphDrawerMap.GetInstance().GetDrawerType(proxyPortData.m_ProxyPortData.GetType());
            BasePortDrawer portDrawer = Activator.CreateInstance(drawerType) as BasePortDrawer;
            return portDrawer!.CreatePortContentView(proxyPortData.m_ProxyPortData, serializedProperty.FindPropertyRelative("m_ProxyPortData"), out portView);
        }
    }
}