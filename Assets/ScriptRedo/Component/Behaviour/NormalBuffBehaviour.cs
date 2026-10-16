using System;

namespace ScriptRedo
{
    public sealed class NormalBuffBehaviour : IBehaviour
    {
        private PropertyComponent m_PropertyComponent;

        private Property m_Influence;

        private Property m_Layer;

        private PropertyType m_InfluencePropertyType;

        private float m_EachLayerDuration;

        private float m_ModifyValue;

        public void OnAddToEntity(Entity entity)
        {
            m_PropertyComponent = entity.GetComponent<PropertyComponent>();
            if (m_PropertyComponent != null)
            {
                //获取Health属性同理
                m_Influence = m_PropertyComponent.GetProperty(m_InfluencePropertyType);
                m_Layer = m_PropertyComponent.GetProperty(PropertyType.Layer);
                //如果PropertyComponent存在也需要给这个组件注册事件
                m_PropertyComponent.RegisterPropertyChangeListener(m_InfluencePropertyType, ListenInfluencePropertyChange);
                m_PropertyComponent.RegisterPropertyChangeListener(PropertyType.Layer, ListenLayerPropertyChange);
            }
            entity.RegisterComponentReferenceChangeListener(typeof(PropertyComponent), ListenPropertyComponentChange);
        }

        public void OnRemoveFromEntity(Entity entity)
        {
            entity.UnregisterComponentReferenceChangeListener(typeof(PropertyComponent), ListenPropertyComponentChange);
            if (m_PropertyComponent != null)
            {
                //同样需要取消注册获取Property的事件，以避免内存泄露：当前组件都被移除了，PropertyComponent改变属性后更新当前组件
                m_PropertyComponent.UnregisterPropertyChangeListener(m_InfluencePropertyType, ListenInfluencePropertyChange);
                m_PropertyComponent.UnregisterPropertyChangeListener(PropertyType.Layer, ListenLayerPropertyChange);
                //这里置空PropertyComponent组件是为了避免当前组件先于PropertyComponent移除，没有执行NotifyChange，导致一直引用PropertyComponent
                m_PropertyComponent = null;
            }
            m_Influence = null;
            m_Layer = null;
        }

        public bool IsRunning()
        {
            throw new NotImplementedException();
        }

        public void Start()
        {
            //开启计时器
        }

        public void Stop()
        {
            //停止计时器
        }

        public void Reset()
        {
            //重置计时器
        }

        private void DoInfluenceProperty()
        {
            if (m_Influence != null)
            {
                m_Influence.ModifyCurrentValue(nameof(NormalBuffBehaviour), m_ModifyValue);
            }
        }

        private void ListenPropertyComponentChange(IComponent component)
        {
            if (component != null)
            {
                m_PropertyComponent = (PropertyComponent)component;
                m_PropertyComponent.RegisterPropertyChangeListener(PropertyType.Layer, ListenLayerPropertyChange);
                m_PropertyComponent.RegisterPropertyChangeListener(m_InfluencePropertyType, ListenInfluencePropertyChange);
            }
            else
            {
                if (m_PropertyComponent != null)
                {
                    m_PropertyComponent.UnregisterPropertyChangeListener(PropertyType.Layer, ListenLayerPropertyChange);
                    m_PropertyComponent.UnregisterPropertyChangeListener(m_InfluencePropertyType, ListenInfluencePropertyChange);
                    m_PropertyComponent = null;
                }
            }
        }

        private void ListenLayerPropertyChange(Property layerProperty)
        {
            m_Layer = layerProperty;
        }

        private void ListenInfluencePropertyChange(Property influenceProperty)
        {
            m_Influence = influenceProperty;
        }
    }
}