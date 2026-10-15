using System;
using System.Collections.Generic;
using System.Text;

namespace ScriptRedo
{
    public abstract class BaseValue<T> where T : struct, IEquatable<T>
    {
        protected T m_MaxValue;

        protected T m_MinValue;

        protected T m_CurrentValue;

        private bool m_IsEnableRecordMaxValue;

        private bool m_IsEnableRecordMinValue;

        private bool m_IsEnableRecordCurrentValue;

        public void Initialize(T maxValue, T minValue, T currentValue, bool isEnableRecordMaxValue, bool isEnableRecordMinValue, bool isEnableRecordCurrentValue)
        {
            m_IsEnableRecordMaxValue = isEnableRecordMaxValue;
            m_IsEnableRecordMinValue = isEnableRecordMinValue;
            m_IsEnableRecordCurrentValue = isEnableRecordCurrentValue;
            OnInitialize(maxValue, minValue, currentValue);
        }

        public void Release()
        {
            ValueRecord.RemoveRecords(this);
            OnRelease();
        }

        public T GetMaxValue()
        {
            return m_MaxValue;
        }

        public T GetMinValue()
        {
            return m_MinValue;
        }

        public T GetCurrentValue()
        {
            return m_CurrentValue;
        }

        public void ModifyMaxValue(string modifier, T delta)
        {
            if (delta.Equals(default))
            {
                return;
            }
            T actualModifiedValue = ProtectedModifyMaxValue(modifier, delta);
            Record(ValueConstraintType.Max, modifier, delta, actualModifiedValue);
        }

        public void ModifyMinValue(string modifier, T delta)
        {
            if (delta.Equals(default))
            {
                return;
            }
            T actualModifiedValue = ProtectedModifyMinValue(modifier, delta);
            Record(ValueConstraintType.Min, modifier, delta, actualModifiedValue);
        }

        public void ModifyCurrentValue(string modifier, T delta)
        {
            if (delta.Equals(default))
            {
                return;
            }
            T actualModifiedValue = ProtectedModifyCurrentValue(modifier, delta);
            Record(ValueConstraintType.Current, modifier, delta, actualModifiedValue);
        }

        public void SetIsEnableRecordValue(ValueConstraintType valueConstraintType, bool isEnableRecord)
        {
            ref bool isEnabledRecord = ref GetIsEnableRecordValue(valueConstraintType);
            if (isEnabledRecord != isEnableRecord)
            {
                if (!isEnableRecord)
                {
                    ValueRecord.RemoveRecordByValueConstraintType(this, valueConstraintType);
                }
                isEnabledRecord = isEnableRecord;
            }
        }

        private ref bool GetIsEnableRecordValue(ValueConstraintType valueConstraintType)
        {
            switch (valueConstraintType)
            {
                case ValueConstraintType.Max:
                    return ref m_IsEnableRecordMaxValue;
                case ValueConstraintType.Min:
                    return ref m_IsEnableRecordMinValue;
                case ValueConstraintType.Current:
                    return ref m_IsEnableRecordCurrentValue;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        protected void Record(ValueConstraintType valueConstraintType, string modifier, T expectedModifiedValue, T actualModifiedValue)
        {
            if (GetIsEnableRecordValue(valueConstraintType))
            {
                ValueRecord.AddRecord(this, ValueRecord.Allocate(modifier, valueConstraintType, expectedModifiedValue, actualModifiedValue));
            }
        }

        protected abstract void OnInitialize(T maxValue, T minValue, T currentValue);

        protected abstract void OnRelease();

        protected abstract T ProtectedModifyMaxValue(string modifier, T delta);

        protected abstract T ProtectedModifyMinValue(string modifier, T delta);

        protected abstract T ProtectedModifyCurrentValue(string modifier, T delta);

        private sealed class ValueRecord
        {
            private T ActualModifiedValue;

            private T ExpectedModifiedValue;

            private string Modifier;

            private ValueConstraintType ValueConstraintType;

            private static readonly Stack<ValueRecord> s_Pool = new();

            private static readonly Dictionary<BaseValue<T>, List<ValueRecord>> s_ValueRecords = new();

            private static readonly StringBuilder s_StringBuilder = new();

            public static ValueRecord Allocate(string modifier, ValueConstraintType valueConstraintType, T expectedModifiedValue, T actualModifiedValue)
            {
                ValueRecord valueRecord = s_Pool.Count > 0 ? s_Pool.Pop() : new ValueRecord();
                valueRecord.Modifier = modifier;
                valueRecord.ValueConstraintType = valueConstraintType;
                valueRecord.ExpectedModifiedValue = expectedModifiedValue;
                valueRecord.ActualModifiedValue = actualModifiedValue;
                return valueRecord;
            }

            private static void Free(ValueRecord valueRecord)
            {
                s_Pool.Push(valueRecord);
            }

            public static void AddRecord(BaseValue<T> valueInstance, ValueRecord record)
            {
                if (!s_ValueRecords.TryGetValue(valueInstance, out List<ValueRecord> valueRecords))
                {
                    valueRecords = new List<ValueRecord>();
                    s_ValueRecords.Add(valueInstance, valueRecords);
                }
                valueRecords.Add(record);
            }

            public static void RemoveRecordByValueConstraintType(BaseValue<T> valueInstance, ValueConstraintType valueConstraintType)
            {
                if (s_ValueRecords.TryGetValue(valueInstance, out List<ValueRecord> valueRecords))
                {
                    int index = 0;
                    while (index < valueRecords.Count)
                    {
                        if (valueRecords[index].ValueConstraintType == valueConstraintType)
                        {
                            int lastIndex = valueRecords.Count - 1;
                            Free(valueRecords[index]);
                            (valueRecords[index], valueRecords[lastIndex]) = (valueRecords[lastIndex], valueRecords[index]);
                            valueRecords.RemoveAt(lastIndex);
                        }
                        else
                        {
                            index++;
                        }
                    }
                }
            }

            public static void RemoveRecords(BaseValue<T> valueInstance)
            {
                if (s_ValueRecords.Remove(valueInstance, out List<ValueRecord> valueRecords))
                {
                    for (int i = 0; i < valueRecords.Count; i++)
                    {
                        Free(valueRecords[i]);
                    }
                }
            }

            public static string LogValueRecords(BaseValue<T> valueInstance)
            {
                if (s_ValueRecords.TryGetValue(valueInstance, out List<ValueRecord> valueRecords))
                {
                    s_StringBuilder.Clear();
                    for (int i = 0; i < valueRecords.Count; i++)
                    {
                        AppendValueRecord(valueRecords[i]);
                    }
                    return s_StringBuilder.ToString();
                }
                return null;
            }

            public static string LogValueRecords(BaseValue<T> valueInstance, ValueConstraintType valueConstraintType)
            {
                if (s_ValueRecords.TryGetValue(valueInstance, out List<ValueRecord> valueRecords))
                {
                    s_StringBuilder.Clear();
                    for (int i = 0; i < valueRecords.Count; i++)
                    {
                        if (valueRecords[i].ValueConstraintType == valueConstraintType)
                        {
                            AppendValueRecord(valueRecords[i]);
                        }
                    }
                    return s_StringBuilder.ToString();
                }
                return null;
            }

            private static void AppendValueRecord(ValueRecord valueRecord)
            {
                s_StringBuilder.Append("Value constraint type:");
                s_StringBuilder.Append(valueRecord.ValueConstraintType);
                s_StringBuilder.Append("Modifier:");
                s_StringBuilder.Append(valueRecord.Modifier);
                s_StringBuilder.Append("Expected modified value:");
                s_StringBuilder.Append(valueRecord.ExpectedModifiedValue);
                s_StringBuilder.Append("Actual modified value");
                s_StringBuilder.Append(valueRecord.ActualModifiedValue);
                s_StringBuilder.Append("\n");
            }
        }
    }
}