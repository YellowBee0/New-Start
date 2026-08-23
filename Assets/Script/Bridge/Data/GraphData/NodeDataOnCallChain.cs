using System.Collections.Generic;

namespace YBFramework.Bridge.NewData
{
    public class NodeDataOnCallChain
    {
        private readonly HashSet<BasePortData> m_PortsDataOnCallChain = new();

        public bool Contains(BasePortData portData)
        {
            return m_PortsDataOnCallChain.Contains(portData);
        }
        
        public bool AddPortDataOnCallChain(BasePortData portDataOnCallChain)
        {
            return m_PortsDataOnCallChain.Add(portDataOnCallChain);
        }

        public bool RemovePortDataOnCallChain(BasePortData portDataOnCallChain)
        {
            return m_PortsDataOnCallChain.Remove(portDataOnCallChain);
        }

        public HashSet<BasePortData>.Enumerator GetPortsDataOnCallChain()
        {
            return m_PortsDataOnCallChain.GetEnumerator();
        }
    }
}