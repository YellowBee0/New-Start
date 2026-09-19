using System;
using System.Collections.Generic;

namespace YBFramework.Bridge.Data
{
    public class CheckNodeExecutionContext
    {
        #region Pool

        private static readonly Dictionary<Type, Stack<CheckNodeExecutionContext>> s_Pools = new();

        public static T Allocate<T>(BaseNodeData nodeData, ExecutableNodeData executableNodeData) where T : CheckNodeExecutionContext
        {
            Type type = typeof(T);
            if (!s_Pools.TryGetValue(type, out Stack<CheckNodeExecutionContext> pool))
            {
                pool = new Stack<CheckNodeExecutionContext>();
                s_Pools.Add(type, pool);
            }
            CheckNodeExecutionContext checkNodeExecutionContext = pool.Count > 0 ? pool.Pop() : new CheckNodeExecutionContext();
            checkNodeExecutionContext.NodeData = nodeData;
            checkNodeExecutionContext.CheckedPortsData.Clear();
            checkNodeExecutionContext.ExecutableNodeData = executableNodeData;
            return (T)checkNodeExecutionContext;
        }

        public static void Release(CheckNodeExecutionContext checkNodeExecutionContext)
        {
            Type type = checkNodeExecutionContext.GetType();
            if (s_Pools.TryGetValue(type, out Stack<CheckNodeExecutionContext> pool))
            {
                pool.Push(checkNodeExecutionContext);
            }
        }

        #endregion

        protected CheckNodeExecutionContext()
        {
            CheckedPortsData = new HashSet<BasePortData>();
        }

        public readonly HashSet<BasePortData> CheckedPortsData;

        public BaseNodeData NodeData;

        public ExecutableNodeData ExecutableNodeData;

        protected virtual void OnRelease()
        {
            CheckedPortsData.Clear();
        }
    }
}