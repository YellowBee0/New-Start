using System.Collections.Generic;

namespace ScriptRedo
{
    public static class CommonPool<T> where T : class, IPooled, new()
    {
        private static readonly Stack<T> s_Pool;

        private static readonly PoolAnalyzerAdapter s_PoolAnalyzerAdapter;

        static CommonPool()
        {
            s_Pool = new Stack<T>();
            s_PoolAnalyzerAdapter = new PoolAnalyzerAdapter
            {
                ObjectType = typeof(T)
            };
            //在这里添加AnalyzablePoolAdapter到AnalyzablePool中去
        }

        public static T Allocate()
        {
            T obj;
            if (s_Pool.Count > 0)
            {
                obj = s_Pool.Pop();
            }
            else
            {
                obj = new T();
                s_PoolAnalyzerAdapter.TotalCount++;
            }
            return obj;
        }

        public static void Release(T obj)
        {
            if (obj != null)
            {
                obj.ReleaseResource();
                s_Pool.Push(obj);
                s_PoolAnalyzerAdapter.IdleCount++;
            }
        }
    }
}