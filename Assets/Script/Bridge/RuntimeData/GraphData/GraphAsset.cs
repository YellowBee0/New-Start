using System.Collections.Generic;
using UnityEngine;

namespace YBFramework.Bridge.NewData
{
    public sealed class GraphAsset : ScriptableObject
    {
        [SerializeReference] private List<BaseNodeData> m_NodesData;

        public IReadOnlyList<BaseNodeData> GetNodesData()
        {
            return m_NodesData;
        }

        public BaseNodeData FindNodeData(int nodeID)
        {
            for (int i = 0; i < m_NodesData.Count; i++)
            {
                BaseNodeData nodeData = m_NodesData[i];
                if (nodeData.GetNodeID() == nodeID)
                {
                    return nodeData;
                }
            }
            return null;
        }
    }
}