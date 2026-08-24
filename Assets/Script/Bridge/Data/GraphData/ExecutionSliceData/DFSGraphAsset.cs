using System.Collections.Generic;

namespace YBFramework.Bridge.NewData
{
    public sealed class DFSGraphAsset
    {
        private static readonly Stack<DFSGraphAsset> s_Pool = new();

        public static DFSGraphAsset Allocate(GraphAsset graphAsset, Dictionary<BaseNodeData, NodeSliceData> nodesSliceData)
        {
            DFSGraphAsset dfsGraphAsset = s_Pool.Count > 0 ? s_Pool.Pop() : new DFSGraphAsset();
            dfsGraphAsset.m_GraphAsset = graphAsset;
            dfsGraphAsset.m_NodesSliceData = nodesSliceData;
            return dfsGraphAsset;
        }

        public static void Free(DFSGraphAsset dfsGraphAsset)
        {
            dfsGraphAsset.m_GraphAsset = null;
            dfsGraphAsset.m_NodesSliceData = null;
            dfsGraphAsset.m_Parent = null;
            s_Pool.Push(dfsGraphAsset);
        }

        private GraphAsset m_GraphAsset;

        private Dictionary<BaseNodeData, NodeSliceData> m_NodesSliceData;
        
        public DFSNodeData DFSNodeData;

        private DFSGraphAsset m_Parent;

        public GraphAsset GetGraphAsset()
        {
            return m_GraphAsset;
        }

        public Dictionary<BaseNodeData, NodeSliceData> GetNodesSliceData()
        {
            return m_NodesSliceData;
        }

        public DFSGraphAsset GetParent()
        {
            return m_Parent;
        }

        public void SetParent(DFSGraphAsset parent)
        {
            m_Parent = parent;
        }
    }
}