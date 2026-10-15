using System;
using System.Collections.Generic;

namespace ScriptRedo
{
    public sealed class PropertyComponent : IComponent
    {
        private readonly List<Property> m_Properties = new();

        private ReferenceChangeListener<PropertyType, Property> m_PropertyChangeListener;

        public void InitializeData(PropertyComponentData componentData)
        {
            //在此初始话m_Properties
        }

        public void OnAddToEntity(Entity entity)
        {
            if (m_PropertyChangeListener != null)
            {
                for (int i = 0; i < m_Properties.Count; i++)
                {
                    Property property = m_Properties[i];
                    m_PropertyChangeListener.NotifyChange(property.GetPropertyType(), property);
                }
            }
        }

        public void OnRemoveFromEntity(Entity entity)
        {
            if (m_PropertyChangeListener != null)
            {
                //循环暂时这么做，之后Property是会池化，需要释放每一个Property
                for (int i = 0; i < m_Properties.Count; i++)
                {
                    m_PropertyChangeListener.NotifyChange(m_Properties[i].GetPropertyType(), null);
                }
            }
            m_Properties.Clear();
        }

        public Property GetProperty(PropertyType propertyType)
        {
            for (int i = 0; i < m_Properties.Count; i++)
            {
                Property property = m_Properties[i];
                if (property.GetPropertyType() == propertyType)
                {
                    return property;
                }
            }
            return null;
        }

        public void AddProperty(Property property)
        {
            PropertyType propertyType = property.GetPropertyType();
            for (int i = 0; i < m_Properties.Count; i++)
            {
                if (m_Properties[i].GetPropertyType() == propertyType)
                {
                    return;
                }
            }
            m_Properties.Add(property);
            if (m_PropertyChangeListener != null)
            {
                m_PropertyChangeListener.NotifyChange(propertyType, property);
            }
        }

        public void RemoveProperty(Property property)
        {
            if (m_Properties.Remove(property))
            {
                if (m_PropertyChangeListener != null)
                {
                    m_PropertyChangeListener.NotifyChange(property.GetPropertyType(), property);
                }
            }
        }

        public void RegisterPropertyChangeListener(PropertyType propertyType, Action<Property> listener)
        {
            m_PropertyChangeListener ??= new ReferenceChangeListener<PropertyType, Property>();
            m_PropertyChangeListener.RegisterChangeListener(propertyType, listener);
        }

        public void UnregisterPropertyChangeListener(PropertyType propertyType, Action<Property> listener)
        {
            if (m_PropertyChangeListener != null)
            {
                m_PropertyChangeListener.UnregisterChangeListener(propertyType, listener);
            }
        }
    }
}