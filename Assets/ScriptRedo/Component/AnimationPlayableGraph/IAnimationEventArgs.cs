using System;
using UnityEngine;

namespace ScriptRedo
{
    //参数的数据只能在编辑器中编辑器，反序列化后不再改变（只读）
    public sealed class NoneAnimationEventArg : IRuntimeData<NoneArgAnimationEventInvoker>
    {
        public NoneArgAnimationEventInvoker CreateRuntimeInstance()
        {
            return new NoneArgAnimationEventInvoker();
        }
    }

    [Serializable]
    public sealed class OneAnimationEventArg<T1> : IRuntimeData<OneArgAnimationEventInvoker<T1>>
    {
        [SerializeField] private T1 Arg1;

        public OneArgAnimationEventInvoker<T1> CreateRuntimeInstance()
        {
            OneArgAnimationEventInvoker<T1> invoker = new();
            invoker.InitializeArg(this);
            return invoker;
        }

        public T1 GetArg()
        {
            return Arg1;
        }
    }

    [Serializable]
    public sealed class TwoAnimationEventArgs<T1, T2> : IRuntimeData<TwoArgsAnimationEventInvoker<T1, T2>>
    {
        [SerializeField] private T1 Arg1;

        [SerializeField] private T2 m_Arg2;

        public TwoArgsAnimationEventInvoker<T1, T2> CreateRuntimeInstance()
        {
            TwoArgsAnimationEventInvoker<T1, T2> invoker = new();
            invoker.InitializeArgs(this);
            return invoker;
        }

        public T1 GetArg1()
        {
            return Arg1;
        }

        public T2 GetArg2()
        {
            return m_Arg2;
        }
    }
}