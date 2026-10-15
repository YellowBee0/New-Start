using System;
using System.Collections.Generic;

namespace ScriptRedo
{
    public sealed class Entity
    {
        private readonly Dictionary<Type, IComponent> m_Components = new();

        //如果Entity要复用，那么这个东西也得归还
        private ReferenceChangeListener<Type, IComponent> m_ComponentReferenceChangeListener;

        /// <summary>
        /// 获取指定类型的组件
        /// </summary>
        /// <typeparam name="T">组件泛型</typeparam>
        /// <returns>组件实例</returns>
        public T GetComponent<T>() where T : IComponent
        {
            return (T)GetComponent(typeof(T));
        }

        /// <summary>
        /// 获取指定类型的组件
        /// </summary>
        /// <param name="type">组件类型</param>
        /// <returns>组件实例</returns>
        public IComponent GetComponent(Type type)
        {
            return m_Components.GetValueOrDefault(type);
        }

        /// <summary>
        /// 添加组件到实体
        /// </summary>
        /// <param name="component">添加的组件实例</param>
        /// <typeparam name="T">添加的组件泛型</typeparam>
        /// <returns>是否添加成功</returns>
        public bool AddComponent<T>(T component) where T : IComponent
        {
            return component != null && AddComponent(typeof(T), component);
        }

        /// <summary>
        /// 添加组件到Entity
        /// </summary>
        /// <param name="component">添加的组件实例</param>
        /// <returns>是否添加成功</returns>
        public bool AddComponent(IComponent component)
        {
            return AddComponent(component.GetType(), component);
        }

        /// <summary>
        /// 从Entity移除组件，移除成功必然调用IComponent的OnRemoveFromEntity
        /// </summary>
        /// <typeparam name="T">移除的组件泛型</typeparam>
        /// <returns>是否移除成功</returns>
        public bool RemoveComponent<T>() where T : IComponent
        {
            return PrivateRemoveComponent(typeof(T));
        }

        /// <summary>
        /// 从Entity移除组件，移除成功必然调用IComponent的OnRemoveFromEntity
        /// </summary>
        /// <param name="type">移除组件的类型</param>
        /// <returns>是否移除成功</returns>
        public bool RemoveComponent(Type type)
        {
            return PrivateRemoveComponent(type);
        }

        /// <summary>
        /// 从Entity移除组件
        /// </summary>
        /// <param name="component">移除的组件实例</param>
        /// <returns>是否移除成功</returns>
        public bool RemoveComponent(IComponent component)
        {
            return PrivateRemoveComponent(component.GetType());
        }

        public void RegisterComponentReferenceChangeListener(Type componentType, Action<IComponent> listener)
        {
            m_ComponentReferenceChangeListener ??= new ReferenceChangeListener<Type, IComponent>();
            m_ComponentReferenceChangeListener.RegisterChangeListener(componentType, listener);
        }

        public void UnregisterComponentReferenceChangeListener(Type componentType, Action<IComponent> listener)
        {
            if (m_ComponentReferenceChangeListener != null)
            {
                m_ComponentReferenceChangeListener.UnregisterChangeListener(componentType, listener);
            }
        }

        /// <summary>
        /// 从Entity移除组件的私有调用
        /// </summary>
        /// <param name="type">移除组件的类型</param>
        /// <returns>是否移除成功</returns>
        private bool PrivateRemoveComponent(Type type)
        {
            if (m_Components.Remove(type, out IComponent component))
            {
                //先调用组件的OnRemoveFromEntity，再调用NotifyChange的原因：组件有可能也管理其他的生命周期（比如PropertyComponent），在组件被移除时，组件管理的生命周期同样也得移除
                //移除逻辑一般写在OnRemoveFromEntity中，所以先调用OnRemoveFromEntity
                component.OnRemoveFromEntity(this);
                if (m_ComponentReferenceChangeListener != null)
                {
                    m_ComponentReferenceChangeListener.NotifyChange(type, null);
                }
                return true;
            }
            return false;
        }

        /// <summary>
        /// 添加组件到Entity的私有调用
        /// </summary>
        /// <param name="type">添加的组件类型</param>
        /// <param name="component">添加的组件实例</param>
        /// <returns>是否添加成功</returns>
        private bool AddComponent(Type type, IComponent component)
        {
            if (m_Components.TryAdd(type, component))
            {
                //先调用NotifyChange，再调用组件的OnAddToEntity的原因：组件存在管理生命周期（比如Property），想要获取组件管理生命周期的对象需要先保证组件被加载，然后再获取这些内容
                if (m_ComponentReferenceChangeListener != null)
                {
                    m_ComponentReferenceChangeListener.NotifyChange(type, component);
                }
                component.OnAddToEntity(this);
                return true;
            }
            return false;
        }
    }
}