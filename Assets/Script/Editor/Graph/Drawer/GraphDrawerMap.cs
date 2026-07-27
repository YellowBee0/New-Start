using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace YBFramework.Editor.Graph
{
    public sealed class GraphDrawerMap
    {
        private static readonly GraphDrawerMap s_Instance = new();

        public static GraphDrawerMap GetInstance()
        {
            return s_Instance;
        }

        private readonly Dictionary<Type, Type> m_GraphDrawers = new();

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