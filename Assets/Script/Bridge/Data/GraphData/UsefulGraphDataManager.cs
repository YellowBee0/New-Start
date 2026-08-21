using System.Collections.Generic;

namespace YBFramework.Bridge.NewData
{
    public sealed class UsefulGraphDataManager
    {
        private readonly Dictionary<GraphAsset, Dictionary<BaseNodeData, HashSet<BasePortData>>> m_UsefulGraphsData = new();

        public IReadOnlyDictionary<BaseNodeData, HashSet<BasePortData>> FindUsefulGraphData(GraphAsset graphAsset)
        {
            return m_UsefulGraphsData.GetValueOrDefault(graphAsset);
        }

        public void FilterUsefulData(GraphAsset graphAsset)
        {
            if (graphAsset != null)
            {
                if (FindUsefulGraphData(graphAsset) == null)
                {
                    IReadOnlyList<BaseNodeData> nodesData = graphAsset.GetNodesData();
                    for (int i = 0; i < nodesData.Count; i++)
                    {
                        nodesData[i].FilterUsefulPortData(graphAsset);
                    }
                }
            }
        }
    }
}