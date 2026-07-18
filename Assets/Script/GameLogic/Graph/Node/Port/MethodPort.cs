using System;
using System.Reflection;
using YBFramework.Bridge.Data;

namespace YBFramework.GameLogic.Graph
{
    public sealed class MethodPort : BasePort
    {
        private object m_Target;

        private MethodInfo m_MethodInfo;

        public void InitializeFromData(MethodPortData data)
        {
            m_PortID = data.PortID;
            m_MethodInfo = data.GetMethodInfo();
        }
        
        public void SetTarget(object target)
        {
            m_Target = target;
        }

        //TODO:加上是否需要封装参数
        public Delegate CreateDelegate(Type delegateType)
        {
            return m_MethodInfo == null ? null : m_MethodInfo.CreateDelegate(delegateType, m_Target);
        }
    }
}