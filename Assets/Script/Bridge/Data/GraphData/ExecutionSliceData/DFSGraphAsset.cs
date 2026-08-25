using System.Collections.Generic;

namespace YBFramework.Bridge.NewData
{
    public sealed class DFSGraphAsset
    {
        private static readonly Stack<DFSGraphAsset> s_Pool = new();

        public static DFSGraphAsset Allocate(GraphAsset graphAsset, GraphSliceData graphSliceData)
        {
            DFSGraphAsset dfsGraphAsset = s_Pool.Count > 0 ? s_Pool.Pop() : new DFSGraphAsset();
            dfsGraphAsset.m_GraphAsset = graphAsset;
            dfsGraphAsset.m_GraphSliceData = graphSliceData;
            return dfsGraphAsset;
        }

        public static void Free(DFSGraphAsset dfsGraphAsset)
        {
            dfsGraphAsset.m_GraphAsset = null;
            dfsGraphAsset.m_GraphSliceData = null;
            dfsGraphAsset.m_Parent = null;
            s_Pool.Push(dfsGraphAsset);
        }

        private GraphAsset m_GraphAsset;

        private GraphSliceData m_GraphSliceData;

        public DFSNodeData DFSNodeData;

        private DFSGraphAsset m_Parent;

        public GraphAsset GetGraphAsset()
        {
            return m_GraphAsset;
        }

        public GraphSliceData GetGraphSliceData()
        {
            return m_GraphSliceData;
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