using System;

namespace ScriptRedo
{
    public class TestBehaviour : IBehaviour
    {
        private PropertyComponent m_PropertyComponent;

        private Property m_Health;

        public void OnAddToEntity(Entity entity)
        {
            //先获取一次是为了避免PropertyComponent已经加入Entity中，不会执行NotifyChange，导致一直获取不到PropertyComponent
            m_PropertyComponent = entity.GetComponent<PropertyComponent>();
            if (m_PropertyComponent != null)
            {
                //获取Health属性同理
                m_Health = m_PropertyComponent.GetProperty(PropertyType.Health);
                //如果PropertyComponent存在也需要给这个组件注册事件
                m_PropertyComponent.RegisterPropertyChangeListener(PropertyType.Health, ListenHealthPropertyChange);
            }
            entity.RegisterComponentReferenceChangeListener(typeof(PropertyComponent), ListenPropertyComponentChange);
        }

        public void OnRemoveFromEntity(Entity entity)
        {
            entity.UnregisterComponentReferenceChangeListener(typeof(PropertyComponent), ListenPropertyComponentChange);
            if (m_PropertyComponent != null)
            {
                //同样需要取消注册获取Property的事件，以避免内存泄露：当前组件都被移除了，PropertyComponent改变属性后更新当前组件
                m_PropertyComponent.UnregisterPropertyChangeListener(PropertyType.Health, ListenHealthPropertyChange);
                //这里置空PropertyComponent组件是为了避免当前组件先于PropertyComponent移除，没有执行NotifyChange，导致一直引用PropertyComponent
                m_PropertyComponent = null;
            }
            m_Health = null;
        }

        public bool IsRunning()
        {
            throw new NotImplementedException();
        }

        public void Start()
        {
            throw new NotImplementedException();
        }

        public void Stop()
        {
            throw new NotImplementedException();
        }

        public void Reset()
        {
            throw new NotImplementedException();
        }

        public void InitializeData(TestBehaviourData data)
        {
            
        }

        private void ListenPropertyComponentChange(IComponent component)
        {
            if (component != null)
            {
                m_PropertyComponent = (PropertyComponent)component;
                m_PropertyComponent.RegisterPropertyChangeListener(PropertyType.Health, ListenHealthPropertyChange);
            }
            else
            {
                if (m_PropertyComponent != null)
                {
                    m_PropertyComponent.UnregisterPropertyChangeListener(PropertyType.Health, ListenHealthPropertyChange);
                    m_PropertyComponent = null;
                }
            }
        }

        private void ListenHealthPropertyChange(Property healthProperty)
        {
            m_Health = healthProperty;
        }
    }
}