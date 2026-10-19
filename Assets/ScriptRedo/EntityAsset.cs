using System;
using System.Collections.Generic;
using UnityEngine;

namespace ScriptRedo
{
    public sealed class EntityAsset : ScriptableObject, IRuntimeData<Entity>
    {
        [SerializeReference] private List<IRuntimeData<IComponent>> m_ComponentsData;

        public Entity CreateRuntimeInstance()
        {
            Entity entity = new();
            for (int i = 0; i < m_ComponentsData.Count; i++)
            {
                IComponent component = m_ComponentsData[i].CreateRuntimeInstance();
                entity.AddComponent(component);
            }
            return entity;
        }

        public T GetComponentData<T>() where T : IRuntimeData<IComponent>
        {
            return (T)GetComponentData(typeof(T));
        }

        public IRuntimeData<IComponent> GetComponentData(Type componentDataType)
        {
            for (int i = 0; i < m_ComponentsData.Count; i++)
            {
                Type existComponentDataType = m_ComponentsData[i].GetType();
                if (existComponentDataType == componentDataType)
                {
                    return m_ComponentsData[i];
                }
            }
            return null;
        }
    }
}