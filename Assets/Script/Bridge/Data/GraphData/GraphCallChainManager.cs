using System.Collections.Generic;

namespace YBFramework.Bridge.NewData
{
    public sealed class GraphCallChainManager
    {
        /*private readonly Dictionary<GraphAsset, Dictionary<BaseNodeData, HashSet<BasePortData>>> m_GraphCallChains = new();

        public IReadOnlyDictionary<BaseNodeData, HashSet<BasePortData>> GetGraphCallChain(GraphAsset graphAsset)
        {
            if (!m_GraphCallChains.TryGetValue(graphAsset, out Dictionary<BaseNodeData, HashSet<BasePortData>> graphCallChain))
            {
                if (graphAsset != null)
                {
                    graphCallChain = new Dictionary<BaseNodeData, HashSet<BasePortData>>();
                    IReadOnlyList<BaseNodeData> nodesData = graphAsset.GetNodesData();
                    for (int i = 0; i < nodesData.Count; i++)
                    {
                        nodesData[i].GetCallChain(graphAsset, in graphCallChain);
                    }
                    m_GraphCallChains.Add(graphAsset, graphCallChain);
                }
            }
            return graphCallChain;
        }*/

        private readonly Dictionary<GraphAsset, Dictionary<BaseNodeData, NodeDataOnCallChain>> m_GraphsDataOnCallChain = new();
    }
}