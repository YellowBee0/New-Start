#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace YBFramework.Bridge.Editor
{
    public static class GraphDebugger
    {
        private static readonly Dictionary<Type, Func<Delegate, Action, Action, Delegate>> Factories = new();

        /// <summary>
        /// 包装一个委托调用的函数，每次调用前记录调用开始，调用结束记录结束
        /// </summary>
        /// <param name="target">目标函数赋予的委托</param>
        /// <param name="enter">函数调用前执行的逻辑</param>
        /// <param name="exit">函数调用后执行的逻辑</param>
        /// <returns>Debug封装后的委托</returns>
        /// <exception cref="ArgumentNullException">调用前后执行委托为null</exception>
        public static Delegate Wrap(Delegate target, Action enter, Action exit)
        {
            if (target == null)
            {
                return null;
            }
            if (enter == null)
            {
                throw new ArgumentNullException(nameof(enter));
            }
            if (exit == null)
            {
                throw new ArgumentNullException(nameof(exit));
            }
            Func<Delegate, Action, Action, Delegate> factory;
            lock (Factories)
            {
                Type type = target.GetType();

                if (!Factories.TryGetValue(type, out factory))
                {
                    factory = BuildFactory(type);
                    Factories.Add(type, factory);
                }
            }
            return factory(target, enter, exit);
        }

        private static Func<Delegate, Action, Action, Delegate> BuildFactory(Type type)
        {
            //这里可以优化，因为我的委托端口已经知道自己的委托类型
            MethodInfo invoke = type.GetMethod("Invoke")!;
            if (invoke.ReturnType.IsByRef)
            {
                throw new NotSupportedException("Not supported return type with ref keyword");
            }
            ParameterInfo[] infos = invoke.GetParameters();
            ParameterExpression[] args = new ParameterExpression[infos.Length];
            for (int i = 0; i < args.Length; i++)
            {
                args[i] = Expression.Parameter(infos[i].ParameterType, infos[i].Name);
            }
            ParameterExpression target = Expression.Parameter(typeof(Delegate), "target");
            ParameterExpression enter = Expression.Parameter(typeof(Action), "enter");
            ParameterExpression exit = Expression.Parameter(typeof(Action), "exit");
            // 按原始签名调用，参数和返回值都保持原来的类型。
            InvocationExpression call = Expression.Invoke(Expression.Convert(target, type), args);
            BlockExpression body = Expression.Block(Expression.Invoke(enter), Expression.TryFinally(call, Expression.Invoke(exit)));
            LambdaExpression wrapper = Expression.Lambda(type, body, args);
            // 工厂接收不同的目标和调试回调，创建各自的包装委托。
            return Expression.Lambda<Func<Delegate, Action, Action, Delegate>>(Expression.Convert(wrapper, typeof(Delegate)), target, enter, exit).Compile();
        }
    }
}
#endif