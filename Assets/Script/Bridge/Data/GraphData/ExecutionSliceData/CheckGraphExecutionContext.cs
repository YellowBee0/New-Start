using System.Collections.Generic;

namespace YBFramework.Bridge.Data
{
    public sealed class CheckGraphExecutionContext
    {
        #region Pool

        private static readonly Stack<CheckGraphExecutionContext> s_Pool = new();

        public static CheckGraphExecutionContext Allocate(GraphAsset graphAsset, Dictionary<BaseNodeData, ExecutableNodeData> executableNodesData)
        {
            CheckGraphExecutionContext checkGraphExecutionContext = s_Pool.Count > 0 ? s_Pool.Pop() : new CheckGraphExecutionContext();
            checkGraphExecutionContext.GraphAsset = graphAsset;
            return checkGraphExecutionContext;
        }

        public static void Release(CheckGraphExecutionContext checkGraphExecutionContext)
        {
            checkGraphExecutionContext.GraphAsset = null;
            checkGraphExecutionContext.Parent = null;
            s_Pool.Push(checkGraphExecutionContext);
        }

        #endregion

        public CheckGraphExecutionContext Parent;

        public GraphAsset GraphAsset;

        public Dictionary<BaseNodeData, ExecutableNodeData> ExecutableNodesData;

        public CheckNodeExecutionContext CurrentCheckNodeExecutionContext;
    }
}