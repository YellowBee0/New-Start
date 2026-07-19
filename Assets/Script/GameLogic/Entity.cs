using System;
using System.Collections.Generic;
using UnityEngine;
using YBFramework.Bridge.Data;
using YBFramework.GameLogic.Component;

namespace YBFramework.GameLogic
{
    //TODO:这个Entity可以直接作为一个C#类型，而不是继承MonoBehaviour，那么Entity需要得组件数据应该使用EntityAsset
    // 保存，Entity就是一个空得GameObject作为Root，如果一个Component需要添加什么MonoBehaviour得脚本需要在
    // IComponentData中指定预制体是哪一个，然后每次加载这个组件时实例化预制体在预制体上添加这个脚本
    public sealed class Entity : MonoBehaviour
    {
        //可以池化
        private sealed class Tracker
        {
            public int TrackID;

            public Action OnGetControl;

            public Action OnLoseControl;
        }

        [SerializeReference] private List<IComponentData> m_ComponentData;

        private readonly Dictionary<Type, IComponent> m_Components = new();

        private readonly List<Tracker> m_Trackers = new();

        private bool m_Initialized;

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

        public void Track(ITrackable target, Action onGetControl, Action onLoseControl)
        {
            if (target == null)
            {
                return;
            }
            int trackID = target.GetTrackID();
            Tracker tracker = null;
            for (int i = 0; i < m_Trackers.Count; i++)
            {
                Tracker element = m_Trackers[i];
                if (element.TrackID == trackID)
                {
                    tracker = element;
                    break;
                }
            }
            if (tracker == null)
            {
                tracker = new Tracker
                {
                    TrackID = trackID
                };
                m_Trackers.Add(tracker);
            }
            tracker.OnGetControl += onGetControl;
            tracker.OnLoseControl += onLoseControl;
        }

        public void Untrack(ITrackable target, Action onGetControl, Action onLoseControl)
        {
            if (target == null)
            {
                return;
            }
            int trackID = target.GetTrackID();
            for (int i = 0; i < m_Trackers.Count; i++)
            {
                Tracker tracker = m_Trackers[i];
                if (tracker.TrackID == trackID)
                {
                    tracker.OnGetControl -= onGetControl;
                    tracker.OnLoseControl -= onLoseControl;
                    if (tracker.OnGetControl == null && tracker.OnLoseControl == null)
                    {
                        m_Trackers.RemoveAt(i);
                    }
                }
            }
        }

        public void UntrackAll(ITrackable target)
        {
            if (target == null)
            {
                return;
            }
            int trackID = target.GetTrackID();
            for (int i = 0; i < m_Trackers.Count; i++)
            {
                Tracker tracker = m_Trackers[i];
                if (tracker.TrackID == trackID)
                {
                    m_Trackers.RemoveAt(i);
                }
            }
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
                m_Trackers.Clear();
                EntityManager.UnregisterEntity(this);
                m_Initialized = false;
            }
        }
    }
}