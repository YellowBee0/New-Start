using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace YBFramework.Editor
{
    public sealed class RuntimeToEditorMap
    {
        private static readonly RuntimeToEditorMap s_Instance = new();

        public static RuntimeToEditorMap GetInstance()
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
            TypeCache.TypeCollection types = TypeCache.GetTypesWithAttribute<RuntimeToEditorAttribute>();
            for (int i = 0; i < types.Count; i++)
            {
                Type drawerType = types[i];
                RuntimeToEditorAttribute attribute = drawerType.GetCustomAttribute<RuntimeToEditorAttribute>();
                if (attribute != null)
                {
                    if (!m_GraphDrawers.TryAdd(attribute.RuntimeType, drawerType))
                    {
                        Debug.LogWarning($"{attribute.RuntimeType} is already exists");
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