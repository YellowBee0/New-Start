using System;
using System.Collections.Generic;
using YBFramework.Bridge;

namespace YBFramework.Component
{
    public sealed class ProxyNode : BaseNode
    {
        private readonly List<ProxyPort> m_ProxyPorts = new();

        private Graph m_Graph;

        public override bool Iterator(int index, out BasePort current)
        {
            throw new NotImplementedException();
        }

        public void InitializeFromProxyNodeData(ProxyNodeData data)
        {
            m_Graph = data.GetGraphAsset().CreateGraph();
            IReadOnlyList<ProxyPortData> proxyPortData = data.GetProxyPortData();
            for (int i = 0; i < proxyPortData.Count; i++)
            {
                ProxyPort proxyPort = (ProxyPort)proxyPortData[i].CreateRuntimeInstance();
                m_ProxyPorts.Add(proxyPort);
            }
        }
    }
}