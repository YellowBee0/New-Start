using System.Buffers;
using System.Collections.Generic;
using YBFramework.GameLogic.Graph;

namespace YBFramework.Bridge.Data
{
    public ref struct BuildGraphData
    {
        public readonly GraphAsset GraphAsset;

        public readonly Graph Graph;

        private readonly BuildNodeData[] m_BuildNodesData;

        private int m_Count;

        public BuildGraphData(GraphAsset graphAsset, Graph graph, int size)
        {
            GraphAsset = graphAsset;
            Graph = graph;
            m_BuildNodesData = ArrayPool<BuildNodeData>.Shared.Rent(size);
            m_Count = 0;
        }

        public int GetCount()
        {
            return m_Count;
        }

        public IReadOnlyList<BuildNodeData> GetBuildNodesData()
        {
            return m_BuildNodesData;
        }

        public BuildNodeData FindBuildNodeData(int nodeID)
        {
            for (int i = 0; i < m_Count; i++)
            {
                BuildNodeData buildNodeData = m_BuildNodesData[i];
                if (buildNodeData.NodeData.GetNodeID() == nodeID)
                {
                    return buildNodeData;
                }
            }
            return default;
        }

        public void AddBuildNodeData(BuildNodeData buildNodeData)
        {
            m_BuildNodesData[m_Count++] = buildNodeData;
        }

        public void Dispose()
        {
            for (int i = 0; i < m_Count; i++)
            {
                m_BuildNodesData[i].Dispose();
            }
            ArrayPool<BuildNodeData>.Shared.Return(m_BuildNodesData, true);
        }
    }
}