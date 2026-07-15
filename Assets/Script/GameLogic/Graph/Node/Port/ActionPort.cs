using System;

namespace YBFramework.GameLogic.Graph
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