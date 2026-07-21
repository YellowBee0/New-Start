using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace YBFramework.Editor
{
    public sealed class GraphDrawerMap
    {
        private static readonly GraphDrawerMap s_Instance = new();

        public static GraphDrawerMap GetInstance()
        {
            return s_Instance;
        }

        private readonly Dictionary<Type, Type> m_GraphDrawers = new();

        private readonly Dictionary<Type, Stack<BaseNodeDrawer>> s_NodeDrawers = new();
        
        private readonly Dictionary<Type, Stack<BasePortDrawer>> s_PortDrawers = new();

        public BasePortDrawer GetPortDrawer(Type drawTargetType)
        {
            Type portDrawerType = GetInstance().GetDrawerType(drawTargetType);
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

        public void ReleasePortDrawer(BasePortDrawer portDrawer)
        {
            Type portDrawerType = portDrawer.GetType();
            if (s_PortDrawers.TryGetValue(portDrawerType, out Stack<BasePortDrawer> portDrawers))
            {
                portDrawers.Push(portDrawer);
            }
        }
        
        public BaseNodeDrawer GetNodeDrawer(Type drawTargetType)
        {
            Type nodeDrawerType = GetInstance().GetDrawerType(drawTargetType);
            if (nodeDrawerType == null)
            {
                return null;
            }
            if (!s_NodeDrawers.TryGetValue(nodeDrawerType, out Stack<BaseNodeDrawer> nodeDrawers))
            {
                nodeDrawers = new Stack<BaseNodeDrawer>();
                s_NodeDrawers.Add(nodeDrawerType, nodeDrawers);
            }
            return nodeDrawers.Count > 0 ? nodeDrawers.Pop() : Activator.CreateInstance(nodeDrawerType) as BaseNodeDrawer;
        }

        public void ReleaseNodeDrawer(BaseNodeDrawer nodeDrawer)
        {
            Type nodeDrawerType = nodeDrawer.GetType();
            if (s_NodeDrawers.TryGetValue(nodeDrawerType, out Stack<BaseNodeDrawer> nodeDrawers))
            {
                nodeDrawers.Push(nodeDrawer);
            }
        }

        public void Initialize()
        {
            if (m_GraphDrawers.Count > 0)
            {
                Debug.Log("Graph drawer map has initialized");
                return;
            }
            TypeCache.TypeCollection types = TypeCache.GetTypesWithAttribute<GraphDrawerAttribute>();
            for (int i = 0; i < types.Count; i++)
            {
                Type drawerType = types[i];
                GraphDrawerAttribute attribute = drawerType.GetCustomAttribute<GraphDrawerAttribute>();
                if (attribute != null)
                {
                    if (!m_GraphDrawers.TryAdd(attribute.DrawType, drawerType))
                    {
                        Debug.LogWarning($"{attribute.DrawType} is already exists");
                    }
                }
            }
        }

        public Type GetDrawerType(Type drawType)
        {
            while (drawType != null && drawType != typeof(object))
            {
                Type findType = drawType.IsGenericType ? drawType.GetGenericTypeDefinition() : drawType;
                if (m_GraphDrawers.TryGetValue(findType, out Type drawerType))
                {
                    return drawerType;
                }
                drawType = drawType.BaseType;
            }
            return null;
        }
    }
}