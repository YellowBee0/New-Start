using System.Collections.Generic;

namespace YBFramework.Bridge.Data
{
    public static class GraphCheckResultManager
    {
        private static readonly Dictionary<GraphAsset, Dictionary<BaseNodeData, NodeCheckResult>> s_GraphCheckResults = new();

        public static Dictionary<BaseNodeData, NodeCheckResult> GetGraphCheckResult(GraphAsset graphAsset)
        {
            if (!s_GraphCheckResults.TryGetValue(graphAsset, out Dictionary<BaseNodeData, NodeCheckResult> graphCheckResult))
            {
                graphCheckResult = new Dictionary<BaseNodeData, NodeCheckResult>();
                GraphCheckContext graphCheckContext = new GraphCheckContext
                {
                    GraphAsset = graphAsset,
                    NodeCheckResults = graphCheckResult
                };
                graphCheckContext.StartCheck();
                s_GraphCheckResults.Add(graphAsset, graphCheckResult);
            }
            return graphCheckResult;
        }
    }
}