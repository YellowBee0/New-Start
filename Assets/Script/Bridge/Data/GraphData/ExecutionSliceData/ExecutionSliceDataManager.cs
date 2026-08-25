using System.Collections.Generic;

namespace YBFramework.Bridge.NewData
{
    public sealed class ExecutionSliceDataManager
    {
        private readonly Dictionary<GraphAsset, GraphSliceData> m_GraphSliceData = new();

        public GraphSliceData GetGraphExecutionSliceData(GraphAsset graphAsset)
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
                DFSGraphAsset.Free(dfsGraphAsset);
                m_GraphSliceData.Add(graphAsset, graphSliceData);
            }
            return graphSliceData;
        }
    }
}