using System;
using System.Collections.Generic;
using YBFramework.Bridge.Data;
using YBFramework.Bridge.NewData;

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
            //TODO:这里的GetProxyGraphAsset可能为null
            m_Graph = nodeData.GetProxyGraphAsset().CreateGraph();
            IReadOnlyList<ProxyPortData> proxyPortsData = nodeData.GetProxyPortsData();
            for (int i = 0; i < proxyPortsData.Count; i++)
            {
                ProxyPortData proxyPortData = proxyPortsData[i];
                ProxyPort proxyPort = (ProxyPort)proxyPortData.CreateRuntimeInstance();
                //node需要判空，但一般不为null，这个校验需要在编辑器中完成
                BaseNode node = m_Graph.GetNode(proxyPortData.GetProxyNodeID());
                //port需要判空，但一般不为null，这个校验需要在编辑器中完成

                BasePort proxyTargetPort = node.GetPort(proxyPortData.GetClonedProxyPortData().PortID);
                proxyPort.SetProxyTargetPort(proxyTargetPort);
                proxyTargetPort.MergeData(proxyPortData.GetClonedProxyPortData());
                m_ProxyPorts.Add(proxyPort);
            }
        }

        public void InitializeFromProxyNodeData(SubNodeData nodeData, SubNodeSliceData subNodeSliceData)
        {
        }
    }
}