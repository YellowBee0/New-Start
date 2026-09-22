using System;
using System.Reflection;
using YBFramework.Bridge.Data;
#if UNITY_EDITOR
using YBFramework.Bridge.Editor;
#endif

namespace YBFramework.GameLogic.Graph
{
    public sealed class MethodPort : BasePort
    {
        private object m_Target;

        private MethodInfo m_MethodInfo;

        public void InitializeFromData(MethodPortData data)
        {
            m_MethodInfo = data.GetMethodInfo();
        }

        public void SetTarget(object target)
        {
            m_Target = target;
        }

        //TODO:加上是否需要封装参数
        public Delegate CreateDelegate(Type delegateType)
        {
            Delegate @delegate = m_MethodInfo == null ? null : m_MethodInfo.CreateDelegate(delegateType, m_Target);
#if UNITY_EDITOR
            //TODO:InvokeContext需要当前执行的Graph、调用者节点id和端口id、被调用者节点id和端口id。这些参数何时设置？
            GraphDebugger.InvokeContext incomingContext = new();
            @delegate = GraphDebugger.Wrap(@delegate, incomingContext.OnInvokeEnter, null);
#endif
            return @delegate;
        }
    }
}