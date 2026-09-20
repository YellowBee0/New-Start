using System.Collections.Generic;

namespace YBFramework.Bridge.Data
{
    public sealed class GraphCheckContext
    {
        public GraphCheckContext Parent;

        public GraphAsset GraphAsset;

        public Dictionary<BaseNodeData, NodeCheckResult> NodeCheckResults;

        public BaseNodeData NodeData;

        public NodeCheckResult NodeCheckResult;

        public void StartCheck()
        {
            IReadOnlyList<BaseNodeData> nodesData = GraphAsset.GetNodesData();
            for (int i = 0; i < nodesData.Count; i++)
            {
                nodesData[i].CheckExecutionEntry(this);
            }
        }
    }
}