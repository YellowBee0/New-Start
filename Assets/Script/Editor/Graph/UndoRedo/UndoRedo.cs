using System;
using System.Collections.Generic;

namespace YBFramework.Editor.Graph
{
    public abstract class UndoRedo
    {
        private static readonly Dictionary<Type, Stack<UndoRedo>> s_Pool = new();

        public static T Allocate<T>(int undoGroup) where T : UndoRedo, new()
        {
            Type type = typeof(T);
            if (!s_Pool.TryGetValue(type, out Stack<UndoRedo> pool))
            {
                pool = new Stack<UndoRedo>();
                s_Pool.Add(type, pool);
            }
            T undoRedo = pool.Count > 0 ? (T)pool.Pop() : new T();
            undoRedo.m_UndoGroup = undoGroup;
            return undoRedo;
        }

        public static void Release(UndoRedo undoRedo)
        {
            Type type = undoRedo.GetType();
            if (!s_Pool.TryGetValue(type, out Stack<UndoRedo> pool))
            {
                pool = new Stack<UndoRedo>();
                s_Pool.Add(type, pool);
            }
            pool.Push(undoRedo);
        }

        protected int m_UndoGroup;

        public abstract void Undo();

        public abstract void Redo();
    }
}