using System;
using System.Collections.Generic;

namespace ScriptRedo
{
    public sealed class Entity
    {
        private readonly Dictionary<Type, IComponent> m_Components = new();

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
        /// <param name="doAddToEntityIfSuccess">
        /// 如果添加成功是否执行IComponent的OnAddToEntity。
        /// 一般为true，但是在循环添加组件时，有些组件需要在OnAddToEntity获取其他组件，如果其他组件晚于这个组件添加就获取不到。
        /// 所以需要设置为false，然后在所有组件添加完毕后，手动调用一次添加成功的组件的OnAddToEntity
        /// </param>
        /// <typeparam name="T">添加的组件泛型</typeparam>
        /// <returns>是否添加成功</returns>
        public bool AddComponent<T>(T component, bool doAddToEntityIfSuccess) where T : IComponent
        {
            return component != null && AddComponent(typeof(T), component, doAddToEntityIfSuccess);
        }

        /// <summary>
        /// 添加组件到Entity
        /// </summary>
        /// <param name="component">添加的组件实例</param>
        /// <param name="doAddToEntityIfSuccess">
        /// 如果添加成功是否执行IComponent的OnAddToEntity。
        /// 一般为true，但是在循环添加组件时，有些组件需要在OnAddToEntity获取其他组件，如果其他组件晚于这个组件添加就获取不到。
        /// 所以需要设置为false，然后在所有组件添加完毕后，手动调用一次添加成功的组件的OnAddToEntity
        /// </param>
        /// <returns>是否添加成功</returns>
        public bool AddComponent(IComponent component, bool doAddToEntityIfSuccess)
        {
            return AddComponent(component.GetType(), component, doAddToEntityIfSuccess);
        }

        /// <summary>
        /// 从Entity移除组件，移除成功必然调用IComponent的OnRemoveFromEntity
        /// </summary>
        /// <typeparam name="T">移除的组件泛型</typeparam>
        /// <returns>是否移除成功</returns>
        public bool RemoveComponent<T>() where T : IComponent
        {
            return RemoveComponent(typeof(T), true);
        }

        /// <summary>
        /// 从Entity移除组件，移除成功必然调用IComponent的OnRemoveFromEntity
        /// </summary>
        /// <param name="type">移除组件的类型</param>
        /// <returns>是否移除成功</returns>
        public bool RemoveComponent(Type type)
        {
            return RemoveComponent(type, true);
        }

        /// <summary>
        /// 从Entity移除组件
        /// </summary>
        /// <param name="component">移除的组件实例</param>
        /// <param name="doRemoveFromEntityIfSuccess">
        /// 如果添加成功是否执行IComponent的OnRemoveFromEntity。
        /// 一般为true，仅在需要手动控制IComponent的OnRemoveFromEntity调用时为false
        /// </param>
        /// <returns>是否移除成功</returns>
        public bool RemoveComponent(IComponent component, bool doRemoveFromEntityIfSuccess)
        {
            return RemoveComponent(component.GetType(), doRemoveFromEntityIfSuccess);
        }

        /// <summary>
        /// 从Entity移除组件的私有调用
        /// </summary>
        /// <param name="type">移除组件的类型</param>
        /// <param name="doRemoveFromEntityIfSuccess">
        /// 如果添加成功是否执行IComponent的OnRemoveFromEntity。
        /// 一般为true，仅在需要手动控制IComponent的OnRemoveFromEntity调用时为false
        /// </param>
        /// <returns>是否移除成功</returns>
        private bool RemoveComponent(Type type, bool doRemoveFromEntityIfSuccess)
        {
            if (m_Components.Remove(type, out IComponent component))
            {
                if (doRemoveFromEntityIfSuccess)
                {
                    component.OnRemoveFromEntity(this);
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
        /// <param name="doAddToEntityIfSuccess">
        /// 如果添加成功是否执行IComponent的OnAddToEntity。
        /// 一般为true，但是在循环添加组件时，有些组件需要在OnAddToEntity获取其他组件，如果其他组件晚于这个组件添加就获取不到。
        /// 所以需要设置为false，然后在所有组件添加完毕后，手动调用一次添加成功的组件的OnAddToEntity
        /// </param>
        /// <returns>是否添加成功</returns>
        private bool AddComponent(Type type, IComponent component, bool doAddToEntityIfSuccess)
        {
            if (m_Components.TryAdd(type, component))
            {
                if (doAddToEntityIfSuccess)
                {
                    component.OnAddToEntity(this);
                }
                return true;
            }
            return false;
        }
    }
}