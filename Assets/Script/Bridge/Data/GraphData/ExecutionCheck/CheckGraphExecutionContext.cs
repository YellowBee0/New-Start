using System.Collections.Generic;

namespace YBFramework.Bridge.Data
{
    public sealed class CheckGraphExecutionContext
    {
        #region Pool

        private static readonly Stack<CheckGraphExecutionContext> s_Pool = new();

        public static CheckGraphExecutionContext Allocate(CheckGraphExecutionContext parent, GraphAsset graphAsset, Dictionary<BaseNodeData, CheckNodeExecutionContext> checkNodeExecutionContexts)
        {
            CheckGraphExecutionContext checkGraphExecutionContext = s_Pool.Count > 0 ? s_Pool.Pop() : new CheckGraphExecutionContext();
            checkGraphExecutionContext.GraphAsset = graphAsset;
            checkGraphExecutionContext.CheckNodeExecutionContexts = checkNodeExecutionContexts;
            checkGraphExecutionContext.Parent = parent;
            checkGraphExecutionContext.CurrentCheckNodeExecutionContext = null;
            return checkGraphExecutionContext;
        }

        public static void Release(CheckGraphExecutionContext checkGraphExecutionContext)
        {
            s_Pool.Push(checkGraphExecutionContext);
        }

        #endregion

        public GraphAsset GraphAsset;

        public Dictionary<BaseNodeData, CheckNodeExecutionContext> CheckNodeExecutionContexts;

        public CheckNodeExecutionContext CurrentCheckNodeExecutionContext;

        public CheckGraphExecutionContext Parent;
    }
}