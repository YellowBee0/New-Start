using System;
using System.Collections.Generic;

namespace YBFramework.Editor.NewGraph
{
    public interface IUndoRedoBehaviour
    {
        private static readonly Dictionary<Type, Stack<IUndoRedoBehaviour>> s_Pools = new();

        public static T Allocate<T>() where T : IUndoRedoBehaviour, new()
        {
            Type type = typeof(T);
            if (!s_Pools.TryGetValue(type, out Stack<IUndoRedoBehaviour> pool))
            {
                pool = new Stack<IUndoRedoBehaviour>();
                s_Pools.Add(type, pool);
            }
            return pool.Count > 0 ? (T)pool.Pop() : new T();
        }

        public static void Release(IUndoRedoBehaviour undoRedoBehaviour)
        {
            if (s_Pools.TryGetValue(undoRedoBehaviour.GetType(), out Stack<IUndoRedoBehaviour> pool))
            {
                pool.Push(undoRedoBehaviour);
            }
        }

        void Undo();

        void Redo();
    }
}