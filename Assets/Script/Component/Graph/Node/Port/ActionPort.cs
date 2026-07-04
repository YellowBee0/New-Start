using System;

namespace YBFramework.Component
{
    public sealed class ActionPort : DelegatePort<Action>
    {
        public void Invoke()
        {
            m_Delegate?.Invoke();
        }

        public override object DynamicInvoke(params object[] args)
        {
            Invoke();
            return null;
        }
    }
}