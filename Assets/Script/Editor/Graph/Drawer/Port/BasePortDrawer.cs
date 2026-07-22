using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine.UIElements;
using YBFramework.Bridge.Data;

namespace YBFramework.Editor
{
    [GraphDrawer(typeof(BasePortData))]
    public class BasePortDrawer
    {
        private static readonly Dictionary<Type, Stack<BasePortDrawer>> s_PortDrawers = new();

        public static BasePortDrawer AllocatePortDrawer(Type drawTargetType)
        {
            Type portDrawerType = GraphDrawerMap.GetInstance().GetDrawerType(drawTargetType);
            if (portDrawerType == null)
            {
                return null;
            }
            if (!s_PortDrawers.TryGetValue(portDrawerType, out Stack<BasePortDrawer> portDrawers))
            {
                portDrawers = new Stack<BasePortDrawer>();
                s_PortDrawers.Add(portDrawerType, portDrawers);
            }
            return portDrawers.Count > 0 ? portDrawers.Pop() : Activator.CreateInstance(portDrawerType) as BasePortDrawer;
        }

        public static void ReleasePortDrawer(BasePortDrawer portDrawer)
        {
            Type portDrawerType = portDrawer.GetType();
            if (s_PortDrawers.TryGetValue(portDrawerType, out Stack<BasePortDrawer> portDrawers))
            {
                portDrawers.Push(portDrawer);
            }
        }

        protected BasePortData m_BindPortData;

        public BasePortData GetBindPortData()
        {
            return m_BindPortData;
        }

        public virtual VisualElement CreatePortContentView(BasePortData portData, SerializedProperty serializedProperty, out PortView portView)
        {
            m_BindPortData = portData;
            portView = new PortView(portData.GetPortName(), portData.GetDirection(), portData.GetCapacity(), portData.GetPortColor(), this);
            return portView;
        }
    }
}