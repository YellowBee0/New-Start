using System.Collections.Generic;

namespace YBFramework.Bridge.NewData
{
    public sealed class SubNodeSliceData : NodeSliceData
    {
        private readonly Dictionary<BaseNodeData, NodeSliceData> m_SubGraphSliceData = new();

        public bool ContainsNodeSliceData(BaseNodeData nodeData)
        {
            return m_SubGraphSliceData.ContainsKey(nodeData);
        }
        
        public bool AddSubGraphSliceData(BaseNodeData nodeData, NodeSliceData nodeSliceData)
        {
            return m_SubGraphSliceData.TryAdd(nodeData, nodeSliceData);
        }

        public bool RemoveSubGraphSliceData(BaseNodeData nodeData)
        {
            return m_SubGraphSliceData.Remove(nodeData);
        }

        public Dictionary<BaseNodeData, NodeSliceData> GetNodesSliceData()
        {
            return m_SubGraphSliceData;
        }
        
        public Dictionary<BaseNodeData, NodeSliceData>.Enumerator GetSubGraphSliceData()
        {
            return m_SubGraphSliceData.GetEnumerator();
        }
    }
}