using System.Collections.Generic;
using UnityEngine;
using YBFramework.Component;
#if UNITY_EDITOR
using YBFramework.EditorOnly;
#endif

namespace YBFramework.Bridge
{
    public sealed class GraphAsset : ScriptableObject
    {
        [SerializeReference] private List<BaseNodeData> m_NodeData;

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

        public ushort SourceNodeID;
        
        public GraphType GetGraphType()
        {
            return m_GraphType;
        }
#endif
    }
}