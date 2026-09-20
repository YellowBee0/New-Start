/*using System.Collections.Generic;

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
                CheckGraphExecutionContext checkGraphExecutionContext = CheckGraphExecutionContext.Allocate(graphAsset, graphSliceData);
                IReadOnlyList<BaseNodeData> nodesData = graphAsset.GetNodesData();
                for (int i = 0; i < nodesData.Count; i++)
                {
                    nodesData[i].CheckExecutionEntry(checkGraphExecutionContext);
                }
                CheckGraphExecutionContext.Release(checkGraphExecutionContext);
                m_GraphSliceData.Add(graphAsset, graphSliceData);
            }
            return graphSliceData;
        }
    }
}*/

