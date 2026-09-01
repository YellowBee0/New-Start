using System.Collections.Generic;

namespace YBFramework.Bridge.Data
{
    public class NodeSliceData
    {
        private readonly HashSet<BasePortData> m_PortsSliceData = new();

        private readonly HashSet<string> m_FilteredGraphAssetPaths = new();
        
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

        //TODO:记得在while循环结束后调用Dispose()
        public HashSet<BasePortData>.Enumerator GetPortsSliceData()
        {
            return m_PortsSliceData.GetEnumerator();
        }
    }
}