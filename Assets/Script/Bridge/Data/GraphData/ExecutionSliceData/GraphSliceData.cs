using System.Collections.Generic;

namespace YBFramework.Bridge.NewData
{
    public sealed class GraphSliceData
    {
        private readonly Dictionary<BaseNodeData, NodeSliceData> m_NodesSliceData = new();

        public bool TryGetNodeSliceData(BaseNodeData nodeData, out NodeSliceData nodeSliceData)
        {
            return m_NodesSliceData.TryGetValue(nodeData, out nodeSliceData);
        }

        public void AddNodeSliceData(BaseNodeData nodeData, NodeSliceData nodeSliceData)
        {
            m_NodesSliceData.Add(nodeData, nodeSliceData);
        }

        public bool TryAddNodeSliceData(BaseNodeData nodeData, NodeSliceData nodeSliceData)
        {
            return m_NodesSliceData.TryAdd(nodeData, nodeSliceData);
        }
        
        public bool RemoveNodeSliceData(BaseNodeData nodeData)
        {
            return m_NodesSliceData.Remove(nodeData);
        }

        public Dictionary<BaseNodeData, NodeSliceData>.Enumerator GetNodesSliceDataEnumerator()
        {
            return m_NodesSliceData.GetEnumerator();
        }
    }
}