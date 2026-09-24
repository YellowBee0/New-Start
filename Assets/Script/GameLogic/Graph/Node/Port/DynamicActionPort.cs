using System;
using System.Linq.Expressions;
using System.Reflection;
using YBFramework.Bridge.Data;

namespace YBFramework.GameLogic.Graph
{
    //这个端口只能是单连接，多连接必须保证两次函数参数个数相同且每个参数类型必须相同
    public sealed class DynamicActionPort : BasePort
    {
        private Action<object[]> m_DynamicAction;

        public void Invoke(object[] parameters)
        {
            m_DynamicAction.Invoke(parameters);
        }

        public override void ConnectPort(PortConnectionData portConnectionData, BasePort portToConnect)
        {
            if (portToConnect is MethodPort methodPortToConnect)
            {
                m_DynamicAction = CreateInvoker(methodPortToConnect.GetMethodInfo());
            }
        }
        
        private static Action<object[]> CreateInvoker(MethodInfo method, object target = null)
        {
            if (method == null)
                throw new ArgumentNullException(nameof(method));

            if (method.ReturnType != typeof(void))
                throw new ArgumentException("目标方法必须返回 void。", nameof(method));

            if (method.ContainsGenericParameters)
                throw new ArgumentException("请先指定泛型方法的类型参数。", nameof(method));

            if (!method.IsStatic && target == null)
                throw new ArgumentNullException(nameof(target), "实例方法需要目标对象。");

            ParameterInfo[] parameters = method.GetParameters();
            var args = Expression.Parameter(typeof(object[]), "args");
            var callArgs = new Expression[parameters.Length];

            for (int i = 0; i < parameters.Length; i++)
            {
                Type parameterType = parameters[i].ParameterType;

                if (parameterType.IsByRef)
                    throw new NotSupportedException("此示例不处理 ref/out 参数。");

                // args[i] 从 object 转回方法要求的参数类型
                callArgs[i] = Expression.Convert(
                    Expression.ArrayIndex(args, Expression.Constant(i)),
                    parameterType);
            }

            Expression body = method.IsStatic
                ? Expression.Call(method, callArgs)
                : Expression.Call(
                    Expression.Constant(target, method.DeclaringType),
                    method,
                    callArgs);

            return Expression.Lambda<Action<object[]>>(body, args).Compile();
        }
    }
}