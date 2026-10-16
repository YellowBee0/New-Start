using System;
using System.Collections.Generic;
using System.Text;

namespace ScriptRedo
{
    public abstract class BaseValue<T> : IPooled where T : struct, IEquatable<T>
    {
        protected T m_MaxValue;

        protected T m_MinValue;

        protected T m_CurrentValue;

        private bool m_IsEnableRecordMaxValue;

        private bool m_IsEnableRecordMinValue;

        private bool m_IsEnableRecordCurrentValue;

        public void ReleaseResource()
        {
            if (m_IsEnableRecordMaxValue || m_IsEnableRecordMinValue || m_IsEnableRecordCurrentValue)
            {
                ValueRecord.RemoveRecords(this);
                m_IsEnableRecordMaxValue = false;
                m_IsEnableRecordMinValue = false;
                m_IsEnableRecordCurrentValue = false;
            }
            OnReleaseResource();
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

        private void Record(ValueConstraintType valueConstraintType, string modifier, T expectedModifiedValue, T actualModifiedValue)
        {
            if (GetIsEnableRecordValue(valueConstraintType))
            {
                ValueRecord valRecord = CommonPool<ValueRecord>.Allocate();
                valRecord.InitializeData(modifier, valueConstraintType, expectedModifiedValue, actualModifiedValue);
                ValueRecord.AddRecord(this, valRecord);
            }
        }

        protected abstract void OnReleaseResource();

        protected abstract T ProtectedModifyMaxValue(string modifier, T delta);

        protected abstract T ProtectedModifyMinValue(string modifier, T delta);

        protected abstract T ProtectedModifyCurrentValue(string modifier, T delta);

        private sealed class ValueRecord : IPooled
        {
            private T ActualModifiedValue;

            private T ExpectedModifiedValue;

            private string Modifier;

            private ValueConstraintType ValueConstraintType;

            public void ReleaseResource()
            {
            }

            public void InitializeData(string modifier, ValueConstraintType valueConstraintType, T expectedModifiedValue, T actualModifiedValue)
            {
                Modifier = modifier;
                ValueConstraintType = valueConstraintType;
                ExpectedModifiedValue = expectedModifiedValue;
                ActualModifiedValue = actualModifiedValue;
            }

            private static readonly Dictionary<BaseValue<T>, List<ValueRecord>> s_ValueRecords = new();

            private static readonly StringBuilder s_StringBuilder = new();

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
                            CommonPool<ValueRecord>.Release(valueRecords[index]);
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
                        CommonPool<ValueRecord>.Release(valueRecords[i]);
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