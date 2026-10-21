using System;

namespace ScriptRedo
{
    public interface IAnimationEventInvoker
    {
        void InitializeArg(IAnimationEventArg arg);

        void Invoke();
    }

    #region Instance event invoker

    public sealed class NoneArgAnimationEventInvoker : IAnimationEventInvoker
    {
        private Action m_Action;

        public void InitializeArg(IAnimationEventArg arg)
        {
        }

        public void Invoke()
        {
            m_Action.Invoke();
        }

        public void SetAction(Action action)
        {
            m_Action = action;
        }
    }

    public sealed class OneArgAnimationEventInvoker<T1> : IAnimationEventInvoker
    {
        private Action<T1> m_Action;

        private OneAnimationEventArg<T1> m_Arg;

        public void InitializeArg(IAnimationEventArg arg)
        {
            m_Arg = (OneAnimationEventArg<T1>)arg;
        }

        public void Invoke()
        {
            m_Action.Invoke(m_Arg.GetArg());
        }

        /// <summary>
        /// 设置事件委托。
        /// 目前只使用单播，而不是多播，等之后出现要在同一时间调用同一事件，但是参数值不同的时候考虑做成多播。
        /// </summary>
        /// <param name="action">委托</param>
        public void SetAction(Action<T1> action)
        {
            m_Action = action;
        }
    }

    public sealed class TwoArgsAnimationEventInvoker<T1, T2> : IAnimationEventInvoker
    {
        private Action<T1, T2> m_Action;

        private TwoAnimationEventArgs<T1, T2> m_Args;

        public void InitializeArg(IAnimationEventArg arg)
        {
            m_Args = (TwoAnimationEventArgs<T1, T2>)arg;
        }

        public void Invoke()
        {
            m_Action.Invoke(m_Args.GetArg1(), m_Args.GetArg2());
        }

        public void SetAction(Action<T1, T2> action)
        {
            m_Action = action;
        }
    }

    #endregion

    #region Static event invoker

    public sealed class StaticNoneArgAnimationEventInvoker : IAnimationEventInvoker
    {
        private unsafe delegate*<void> m_Action;

        public void InitializeArg(IAnimationEventArg arg)
        {
        }

        public unsafe void Invoke()
        {
            m_Action();
        }

        public unsafe void SetAction(delegate*<void> action)
        {
            m_Action = action;
        }
    }

    public sealed class StaticOneArgAnimationEventInvoker<T1> : IAnimationEventInvoker
    {
        private unsafe delegate*<T1, void> m_Action;

        private OneAnimationEventArg<T1> m_Arg;

        public void InitializeArg(IAnimationEventArg arg)
        {
            m_Arg = (OneAnimationEventArg<T1>)arg;
        }

        public unsafe void Invoke()
        {
            m_Action(m_Arg.GetArg());
        }

        public unsafe void SetAction(delegate*<T1, void> action)
        {
            m_Action = action;
        }
    }

    public sealed class StaticTwoArgAnimationEventInvoker<T1, T2> : IAnimationEventInvoker
    {
        private unsafe delegate*<T1, T2, void> m_Action;

        private TwoAnimationEventArgs<T1, T2> m_Arg;

        public void InitializeArg(IAnimationEventArg arg)
        {
            m_Arg = (TwoAnimationEventArgs<T1, T2>)arg;
        }

        public unsafe void Invoke()
        {
            m_Action(m_Arg.GetArg1(), m_Arg.GetArg2());
        }

        public unsafe void SetAction(delegate*<T1, T2, void> action)
        {
            m_Action = action;
        }
    }

    #endregion
}