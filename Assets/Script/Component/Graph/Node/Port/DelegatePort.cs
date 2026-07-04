using System;

namespace YBFramework.Component
{
    public abstract class DelegatePort : BasePort
    {
        public abstract Type GetDelegateType();

        public abstract object DynamicInvoke(params object[] args);
    }

    public abstract class DelegatePort<TDelegate> : DelegatePort where TDelegate : Delegate
    {
        protected TDelegate m_Delegate;

        public override Type GetDelegateType()
        {
            return typeof(TDelegate);
        }
    }
}