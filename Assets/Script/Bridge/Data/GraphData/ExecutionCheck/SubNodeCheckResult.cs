using System.Collections.Generic;

namespace YBFramework.Bridge.Data
{
    public sealed class SubNodeCheckResult : NodeCheckResult
    {
        public readonly GraphAsset SubGraphAsset;

        public readonly Dictionary<BaseNodeData, NodeCheckResult> SubNodeCheckResults;

        public SubNodeCheckResult(GraphAsset subGraphAsset)
        {
            SubGraphAsset = subGraphAsset;
            SubNodeCheckResults = new Dictionary<BaseNodeData, NodeCheckResult>();
        }
    }
}