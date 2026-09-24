using System;
using System.Reflection;
using YBFramework.Bridge.Data;

namespace YBFramework.GameLogic.Graph
{
    public sealed class ValueNode<TValue> : BaseNode
    {
        public static readonly MethodInfo GetValueMethod;

        public static readonly MethodInfo SetValueMethod;

        private TValue m_Value;

        private MethodPort m_ReturnValue;

        private MethodPortData m_EnterSetValue;

        private ValueInputPort<TValue> m_SetValueParameter;

        static ValueNode()
        {
            Type methodTargetType = typeof(ValueNode<TValue>);
            GetValueMethod = methodTargetType.GetMethod(nameof(GetValue), BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
            SetValueMethod = methodTargetType.GetMethod(nameof(SetValue), BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
        }

        private TValue GetValue()
        {
            return m_Value;
        }

        private void SetValue()
        {
            m_Value = m_SetValueParameter.Invoke();
        }

        public override void OnStart()
        {
            throw new NotImplementedException();
        }

        public override void OnStop()
        {
            throw new NotImplementedException();
        }

        public override void OnReset()
        {
            throw new NotImplementedException();
        }
    }
}