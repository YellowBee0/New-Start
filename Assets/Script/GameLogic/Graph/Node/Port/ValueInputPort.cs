using System;
using YBFramework.Bridge.Data;

namespace YBFramework.GameLogic.Graph
{
    public sealed class ValueInputPort<TValue> : DelegatePort<Func<TValue>>
    {
        private TValue m_Value;

        public void InitializeFromData(ValueInputPortData<TValue> data)
        {
            m_PortID = data.GetPortID();
            m_Value = data.GetValue();
        }

        public TValue Invoke()
        {
            return m_Delegate == null ? m_Value : m_Delegate.Invoke();
        }

        public override object DynamicInvoke(params object[] args)
        {
            return Invoke();
        }

        public override void MergeData(BasePortData dataToMerge)
        {
            if (dataToMerge is ValueInputPortData<TValue> valueInputPortDataToMerge)
            {
                m_Value = valueInputPortDataToMerge.GetValue();
            }
        }
    }
}