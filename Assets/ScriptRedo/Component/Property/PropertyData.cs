using System;

namespace ScriptRedo
{
    [Serializable]
    public sealed class PropertyData : ISerializableData<Property>
    {
        public float MaxValue;

        public float MinValue;

        public float DefaultValue;

        public PropertyType PropertyType;

        public Property CreateRuntimeInstance()
        {
            Property property = CommonPool<Property>.Allocate();
            property.InitializeData(this);
            return property;
        }
    }
}