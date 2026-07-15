using System;
using System.Collections.Generic;
using UnityEngine;
using YBFramework.Bridge;
using YBFramework.Common;
using YBFramework.GameLogic.Component;

namespace YBFramework.GameLogic
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

        /// <summary>
        /// 初始化Entity，并添加到EntityManager管理，创建自定义组件。
        /// 在Entity Awake的时候必然调用一次，也可以从对象池分配出来时时调用
        /// </summary>
        public void Initialize()
        {
            if (m_Initialized)
            {
                return;
            }
            EntityManager.RegisterEntity(this);
            for (int i = 0; i < m_ComponentData.Count; i++)
            {
                IComponentData componentData = m_ComponentData[i];
                IComponent component = componentData.CreateRuntimeInstance();
                m_Components.Add(componentData.GetRuntimeInstanceType(), component);
                component.SetOwner(this);
            }
            foreach (KeyValuePair<Type, IComponent> kvp in m_Components)
            {
                kvp.Value.OnAdd();
            }
            m_Initialized = true;
        }

        /// <summary>
        /// 清理添加的组件，并移除EntityManager的管理。
        /// 在Entity Destroy的时候必然调用一次，也可以在归还对象池时调用
        /// </summary>
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
                //这里UnregisterTracker，但是
                TrackTargetManager.UnregisterTracker(m_TrackID, ActionClearModel.ClearAll);
                EntityManager.UnregisterEntity(this);
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
            m_TrackID = TrackTargetManager.CombineTrackID(EntityManager.SourceID, m_SourceID);
            Initialize();
        }

        private void OnDestroy()
        {
            Dispose();
        }
    }
}