using System.Collections.Generic;
using UnityEngine;

namespace YBFramework.Bridge
{
    public sealed class NodeAsset : ScriptableObject
    {
        [SerializeReference] private List<BaseNodeData> m_NodeData = new();

        public IEnumerable<BaseNodeData> GetNodeDataEnumerable()
        {
            return m_NodeData;
        }
    }
}