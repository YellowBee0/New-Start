using System.Collections.Generic;
using UnityEngine;
using YBFramework.GameLogic.Graph;
#if UNITY_EDITOR
using YBFramework.EditorOnly;
#endif

namespace YBFramework.Bridge
{
    public sealed class GraphAsset : ScriptableObject
    {
        [SerializeReference] private List<BaseNodeData> m_NodeData;

        public BaseNodeData GetNodeData(int nodeID)
        {
            for (int i = 0; i < m_NodeData.Count; i++)
            {
                BaseNodeData nodeData = m_NodeData[i];
                if (nodeData.NodeID == nodeID)
                {
                    return nodeData;
                }
            }
            return null;
        }
        
        public IReadOnlyList<BaseNodeData> GetNodeData()
        {
            return m_NodeData;
        }
        
        public Graph CreateGraph()
        {
            Graph graph = new();
            graph.InitializeFromGraphAsset(this);
            return graph;
        }
#if UNITY_EDITOR
        [SerializeField] private GraphType m_GraphType;

        public int SourceNodeID;
        
        public GraphType GetGraphType()
        {
            return m_GraphType;
        }
#endif
    }
}