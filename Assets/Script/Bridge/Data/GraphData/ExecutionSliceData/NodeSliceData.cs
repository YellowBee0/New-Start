using System.Collections.Generic;

namespace YBFramework.Bridge.NewData
{
    public class NodeSliceData
    {
        private readonly HashSet<BasePortData> m_PortsSliceData = new();

        public bool ContainsPortSliceData(BasePortData portData)
        {
            return m_PortsSliceData.Contains(portData);
        }

        public bool AddPortSliceData(BasePortData portData)
        {
            return m_PortsSliceData.Add(portData);
        }

        public bool RemovePortSliceData(BasePortData portData)
        {
            return m_PortsSliceData.Remove(portData);
        }

        public HashSet<BasePortData>.Enumerator GetPortsSliceData()
        {
            return m_PortsSliceData.GetEnumerator();
        }
    }
}