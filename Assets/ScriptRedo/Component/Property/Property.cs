using System;

namespace ScriptRedo
{
    public sealed class Property : BaseValue<float>
    {
        private Action m_OnMaxValueChanged;

        private Action m_OnMinValueChanged;

        private Action m_OnCurValueChanged;
        
        private PropertyType m_PropertyType;

        protected override void OnInitialize(float maxValue, float minValue, float currentValue)
        {
            if (maxValue < minValue)
            {
                throw new ArgumentException($"max value: {maxValue} can not less than min value: {minValue}");
            }
            m_MaxValue = maxValue;
            m_MinValue = minValue;
            float expectedCurrentValue = currentValue;
            if (currentValue > maxValue)
            {
                currentValue = maxValue;
            }
            else if (currentValue < minValue)
            {
                currentValue = minValue;
            }
            m_CurrentValue = currentValue;
            Record(ValueConstraintType.Max, "Initialize", maxValue, maxValue);
            Record(ValueConstraintType.Min, "Initialize", minValue, minValue);
            Record(ValueConstraintType.Current, "Initialize", expectedCurrentValue, currentValue);
        }

        protected override void OnRelease()
        {
            m_OnMaxValueChanged = null;
            m_OnMinValueChanged = null;
            m_OnCurValueChanged = null;
        }

        protected override float ProtectedModifyMaxValue(string modifier, float delta)
        {
            float percent = (m_CurrentValue - m_MinValue) / (m_MaxValue - m_MinValue);
            float oldValue = m_MaxValue;
            float newValue = m_MaxValue + delta;
            if (newValue < m_MinValue)
            {
                newValue = m_MinValue;
            }
            m_MaxValue = newValue;
            m_OnMaxValueChanged?.Invoke();
            float actualModifiedValue = newValue - oldValue;
            ModifyCurrentValue(modifier + "(self max value)", percent * actualModifiedValue);
            return actualModifiedValue;
        }

        protected override float ProtectedModifyMinValue(string modifier, float delta)
        {
            float percent = (m_CurrentValue - m_MinValue) / (m_MaxValue - m_MinValue);
            float oldValue = m_MinValue;
            float newValue = m_MinValue + delta;
            if (newValue > m_MaxValue)
            {
                newValue = m_MaxValue;
            }
            m_MinValue = newValue;
            m_OnMinValueChanged?.Invoke();
            float actualModifiedValue = newValue - oldValue;
            ModifyCurrentValue(modifier + "(self min value)", -percent * actualModifiedValue);
            return actualModifiedValue;
        }

        protected override float ProtectedModifyCurrentValue(string modifier, float delta)
        {
            float oldValue = m_CurrentValue;
            float newValue = m_CurrentValue + delta;
            if (newValue > m_MaxValue)
            {
                newValue = m_MaxValue;
            }
            else if (newValue < m_MinValue)
            {
                newValue = m_MinValue;
            }
            m_CurrentValue = newValue;
            m_OnCurValueChanged?.Invoke();
            return oldValue - newValue;
        }

        public PropertyType GetPropertyType()
        {
            return m_PropertyType;
        }

        public void RegisterValueChangeCallBack(ValueConstraintType valueConstraintType, Action callBack)
        {
            switch (valueConstraintType)
            {
                case ValueConstraintType.Max:
                    m_OnMaxValueChanged += callBack;
                    return;
                case ValueConstraintType.Min:
                    m_OnMinValueChanged += callBack;
                    return;
                case ValueConstraintType.Current:
                    m_OnCurValueChanged += callBack;
                    return;
            }
        }

        public void UnregisterValueChangeCallBack(ValueConstraintType valueConstraintType, Action callBack)
        {
            switch (valueConstraintType)
            {
                case ValueConstraintType.Max:
                    m_OnMaxValueChanged -= callBack;
                    return;
                case ValueConstraintType.Min:
                    m_OnMinValueChanged -= callBack;
                    return;
                case ValueConstraintType.Current:
                    m_OnCurValueChanged -= callBack;
                    return;
            }
        }
    }
}