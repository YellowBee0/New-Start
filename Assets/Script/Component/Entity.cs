using System;
using System.Collections.Generic;
using UnityEngine;
using YBFramework.Bridge;
using YBFramework.Common;

namespace YBFramework.Component
{
    public sealed class Entity : MonoBehaviour
    {
        [SerializeReference] private List<IComponentData> m_ComponentData;

        private readonly Dictionary<Type, IComponent> m_Components = new();

        private int m_SourceID;

        private int m_TrackID;

        private bool m_Initialized;

        public int GetTrackID()
        {
            return m_TrackID;
        }

        public void Initialize()
        {
            if (m_Initialized)
            {
                return;
            }
            //这个理不调用TrackTargetManager.NotifyGetControl，因为这一步相当于创建一个新对象，目前还没有需要用到的地方
            for (int i = 0; i < m_ComponentData.Count; i++)
            {
                IComponentData componentData = m_ComponentData[i];
                Type componentType = componentData.GetRuntimeInstanceType();
                IComponent component = componentData.CreateRuntimeInstance();
                m_Components.Add(componentType, component);
                component.SetOwner(this);
            }
            foreach (KeyValuePair<Type, IComponent> kvp in m_Components)
            {
                kvp.Value.OnAdd();
            }
            m_Initialized = true;
        }

        public void Dispose()
        {
            if (m_Initialized)
            {
                foreach (KeyValuePair<Type, IComponent> kvp in m_Components)
                {
                    kvp.Value.OnRemove();
                }
                m_Components.Clear();
                TrackTargetManager.NotifyLoseControl(m_TrackID);
                TrackTargetManager.UnregisterTracker(m_TrackID, NotifyClearModel.ClearAll);
                m_Initialized = false;
            }
        }

        public T GetCustomComponent<T>() where T : IComponent
        {
            Type type = typeof(T);
            if (m_Components.TryGetValue(type, out IComponent component))
            {
                return (T)component;
            }
            return default;
        }

        public void AddCustomComponent(IComponentData componentData)
        {
            Type componentType = componentData.GetRuntimeInstanceType();
            if (!m_Components.ContainsKey(componentType))
            {
                IComponent component = componentData.CreateRuntimeInstance();
                m_Components.Add(componentType, component);
                component.SetOwner(this);
                component.OnAdd();
            }
        }

        public void AddCustomComponent(IComponent component)
        {
            Type componentType = component.GetType();
            if (m_Components.TryAdd(componentType, component))
            {
                component.SetOwner(this);
                component.OnAdd();
            }
        }

        public void RemoveCustomComponent(IComponentData componentData)
        {
            Type componentType = componentData.GetRuntimeInstanceType();
            if (m_Components.Remove(componentType, out IComponent component))
            {
                component.OnRemove();
            }
        }

        public void RemoveCustomComponent(IComponent component)
        {
            Type componentType = component.GetType();
            if (m_Components.Remove(componentType))
            {
                component.OnRemove();
            }
        }

        private void Awake()
        {
            m_SourceID = TrackTargetManager.GenerateSourceID();
            m_TrackID = TrackTargetManager.CombineTrackID(-1, m_SourceID);
            Initialize();
        }

        private void OnDestroy()
        {
            Dispose();
        }
    }
}