using System;

namespace ScriptRedo
{
    public interface IAnimationEventInvoker
    {
        void Invoke();
    }

    public sealed class NoneArgAnimationEventInvoker : IAnimationEventInvoker
    {
        private Action m_Action;

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

        public void InitializeArg(OneAnimationEventArg<T1> arg)
        {
            m_Arg = arg;
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

        public void InitializeArgs(TwoAnimationEventArgs<T1, T2> args)
        {
            m_Args = args;
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
}