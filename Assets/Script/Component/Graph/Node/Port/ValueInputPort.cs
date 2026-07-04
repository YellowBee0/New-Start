using System;
using YBFramework.Bridge;

namespace YBFramework.Component
{
    public sealed class ValueInputPort<TValue> : DelegatePort<Func<TValue>>
    {
        private TValue m_Value;

        public void InitializeFromData(ValueInputPortData<TValue> data)
        {
            m_PortID = data.PortID;
            m_Value = data.Value;
        }

        public TValue Invoke()
        {
            return m_Delegate == null ? m_Value : m_Delegate.Invoke();
        }

        public override object DynamicInvoke(params object[] args)
        {
            return Invoke();
        }
    }
}