using System;
using System.Collections.Generic;
using UnityEngine;

namespace ScriptRedo
{
    [Serializable]
    public sealed class PropertyComponentData : ISerializableData<PropertyComponent>
    {
        [SerializeReference] private List<ISerializableData<Property>> m_PropertiesData;

        public PropertyComponent CreateRuntimeInstance()
        {
            PropertyComponent component = new();
            for (int i = 0; i < m_PropertiesData.Count; i++)
            {
                Property property = m_PropertiesData[i].CreateRuntimeInstance();
            }
            component.InitializeData(this);
            return component;
        }
    }
}