#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;
using UnityEditor;
using YBFramework.GameLogic.Graph;

namespace YBFramework.Bridge.Editor
{
    public static class GraphDebugger
    {
        private static readonly Dictionary<Type, Func<Delegate, Action, Action, Delegate>> Factories = new();

        private static readonly HashSet<Graph> s_RecordGraphs = new();

        private static bool s_IsEnabled;

        private static Action<Graph, int, int, int, int> m_OnInvokeEnter;

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

        public sealed class InvokeContext
        {
            public readonly Graph Graph;

            public readonly int FromNodeID;

            public readonly int FromPortID;

            public readonly int ToNodeID;

            public readonly int ToPortID;

            public void OnInvokeEnter()
            {
                if (s_IsEnabled && s_RecordGraphs.Contains(Graph))
                {
                    //这个委托调用Editor程序集的蓝图编辑器显示执行过程
                    m_OnInvokeEnter.Invoke(Graph, FromNodeID, FromPortID, ToNodeID, ToPortID);
                    //每个Graph实例需要保存一个执行流程的List集合，保存FromNodeID, FromPortID, ToNodeID, ToPortID。
                    //给出一个模式为单帧调试模式，这个模式下当前帧执行了监听的Graph实例就调用
                    //EditorApplication.isPaused = true;
                    //记录当前帧完整的数据，Editor在暂停模式下可以通过调试按钮查看执行流程
                    if (Debugger.IsAttached)
                    {
                        Debugger.Break();
                    }
                }
            }
        }
    }
}
#endif