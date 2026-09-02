using System;
using System.Collections.Generic;
using UnityEngine.UIElements;
using YBFramework.Bridge.Data;

namespace YBFramework.Editor.NewGraph
{
    public abstract class BasePortDrawer
    {
        private BaseNodeDrawer m_NodeDrawer;

        public abstract BasePortData GetPortData();

        public abstract VisualElement GetPortContentView();

        public abstract PortView GetPortView();

        public PortView DrawPortView(BaseNodeDrawer nodeDrawer, BasePortData portData)
        {
            m_NodeDrawer = nodeDrawer;
            PortView portView = OnDrawPortView(portData);
            portView.SetEdgeConnector(nodeDrawer.GetGraphAssetDrawer().GetEdgeConnector());
            return portView;
        }

        protected abstract PortView OnDrawPortView(BasePortData portData);

        protected abstract void OnRelease();

        #region Pool

        private static readonly Dictionary<Type, Stack<BasePortDrawer>> s_Pools = new();

        public static BasePortDrawer Allocate(Type portDataType)
        {
            Type portDrawerType = RuntimeToEditorMap.GetInstance().GetDrawerType(portDataType);
            if (portDrawerType == null)
            {
                return null;
            }
            if (!s_Pools.TryGetValue(portDrawerType, out Stack<BasePortDrawer> pool))
            {
                pool = new Stack<BasePortDrawer>();
                s_Pools.Add(portDrawerType, pool);
            }
            return pool.Count > 0 ? pool.Pop() : Activator.CreateInstance(portDrawerType) as BasePortDrawer;
        }

        public static void Release(BasePortDrawer portDrawer)
        {
            if (s_Pools.TryGetValue(portDrawer.GetType(), out Stack<BasePortDrawer> pool))
            {
                portDrawer.OnRelease();
                pool.Push(portDrawer);
            }
        }

        #endregion
    }
}