using System.Collections.Generic;

namespace YBFramework.Bridge.Data
{
    public static class ExecutionSliceDataManager
    {
        private static readonly Dictionary<GraphAsset, GraphSliceData> m_GraphSliceData = new();

        public static GraphSliceData GetGraphExecutionSliceData(GraphAsset graphAsset)
        {
            if (!m_GraphSliceData.TryGetValue(graphAsset, out GraphSliceData graphSliceData))
            {
                graphSliceData = new GraphSliceData();
                DFSGraphAsset dfsGraphAsset = DFSGraphAsset.Allocate(graphAsset, graphSliceData);
                IReadOnlyList<BaseNodeData> nodesData = graphAsset.GetNodesData();
                for (int i = 0; i < nodesData.Count; i++)
                {
                    nodesData[i].CheckExecutionSliceEntry(dfsGraphAsset);
                }
                DFSGraphAsset.Release(dfsGraphAsset);
                m_GraphSliceData.Add(graphAsset, graphSliceData);
            }
            return graphSliceData;
        }
    }
}