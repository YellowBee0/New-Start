using System;
using System.Collections.Generic;
using YBFramework.Bridge;

namespace YBFramework.GameLogic.Graph
{
    public sealed class ProxyNode : BaseNode
    {
        private readonly List<ProxyPort> m_ProxyPorts = new();

        private Graph m_Graph;

        public override bool Iterator(int index, out BasePort current)
        {
            if (index < m_ProxyPorts.Count)
            {
                current = m_ProxyPorts[index];
                return true;
            }
            current = null;
            return false;
        }

        public override void OnStart()
        {
            throw new NotImplementedException();
        }

        public override void OnStop()
        {
            throw new NotImplementedException();
        }

        public override void OnReset()
        {
            throw new NotImplementedException();
        }

        public void InitializeFromProxyNodeData(ProxyNodeData nodeData)
        {
            m_Graph = nodeData.GetGraphAsset().CreateGraph();
            IReadOnlyList<ProxyPortData> proxyPortData = nodeData.GetProxyPortData();
            for (int i = 0; i < proxyPortData.Count; i++)
            {
                ProxyPortData portData = proxyPortData[i];
                ProxyPort proxyPort = (ProxyPort)portData.CreateRuntimeInstance();
                PortConnectionData address = portData.GetProxyTargetPortAddress();
                BaseNode node = m_Graph.GetNode(address.NodeID);
                //node需要判空，但一般不为null，这个校验需要在编辑器中完成
                BasePort proxyTargetPort = node.GetPort(address.PortID);
                //port需要判空，但一般不为null，这个校验需要在编辑器中完成
                proxyTargetPort.MergeData(portData.GetProxyPortData());
                proxyPort.SetProxyTargetPort(proxyTargetPort);
                m_ProxyPorts.Add(proxyPort);
            }
        }
    }
}