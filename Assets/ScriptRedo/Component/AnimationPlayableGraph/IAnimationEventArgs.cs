using System;
using UnityEngine;

namespace ScriptRedo
{
    //参数的数据只能在编辑器中编辑器，反序列化后不再改变（只读）
    public interface IAnimationEventArg
    {
    }

    [Serializable]
    public sealed class OneAnimationEventArg<T1> : IAnimationEventArg
    {
        [SerializeField] private T1 Arg1;

        public T1 GetArg()
        {
            return Arg1;
        }
    }

    [Serializable]
    public sealed class TwoAnimationEventArgs<T1, T2> : IAnimationEventArg
    {
        [SerializeField] private T1 Arg1;

        [SerializeField] private T2 m_Arg2;

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