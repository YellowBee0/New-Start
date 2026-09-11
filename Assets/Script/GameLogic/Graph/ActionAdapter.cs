using System;

namespace YBFramework.GameLogic.Graph
{
    public sealed class ActionAdapter<TStruct, TClass> where TStruct : struct, TClass where TClass : class
    {
        private Action<TClass> m_Action;

        public ActionAdapter(Action<TClass> action)
        {
            m_Action = action;
        }

        //struct到class转化会涉及到装箱，只要用到这个Adapter都会存在
        public void Invoke(TStruct parameter)
        {
            m_Action.Invoke(parameter);
        }
    }

    public sealed class FuncAdapter<TStruct, TClass> where TStruct : struct, TClass where TClass : class
    {
        private Func<TStruct> m_Func;

        public FuncAdapter(Func<TStruct> func)
        {
            m_Func = func;
        }

        //struct到class转化会涉及到装箱，只要用到这个Adapter都会存在
        public TClass Invoke()
        {
            return m_Func.Invoke();
        }
    }
}