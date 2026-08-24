using System.Collections.Generic;

namespace YBFramework.Bridge.NewData
{
    public sealed class SubNodeDataOnCallChain : NodeDataOnCallChain
    {
        private readonly Dictionary<BaseNodeData, NodeDataOnCallChain> m_SubNodesDataOnCallChain = new();

        public bool AddSubNodeDataOnCallChain(BaseNodeData nodeData, NodeDataOnCallChain nodeDataOnCallChain)
        {
            return m_SubNodesDataOnCallChain.TryAdd(nodeData, nodeDataOnCallChain);
        }

        public bool RemoveSubNodeDataOnCallChain(BaseNodeData nodeData)
        {
            return m_SubNodesDataOnCallChain.Remove(nodeData);
        }

        public Dictionary<BaseNodeData, NodeDataOnCallChain> GetSubNodesDataOnCallChain()
        {
            return m_SubNodesDataOnCallChain;
        }
        
        public Dictionary<BaseNodeData, NodeDataOnCallChain>.Enumerator GetSubNodeDataOnCallChain()
        {
            return m_SubNodesDataOnCallChain.GetEnumerator();
        }
    }
}