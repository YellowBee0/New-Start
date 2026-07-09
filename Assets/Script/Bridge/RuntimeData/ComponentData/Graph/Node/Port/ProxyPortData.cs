using System;
using UnityEngine;
using YBFramework.Component;

namespace YBFramework.Bridge
{
    [Serializable]
    public sealed class ProxyPortData : BasePortData
    {
        [SerializeReference] private BasePortData m_ProxyPortData;

        [SerializeField] private PortConnectionData m_TargetAddress;

        public override BasePort CreateRuntimeInstance()
        {
            ProxyPort proxyPort = new();
            proxyPort.InitializeFromProxyPortData(this);
            return proxyPort;
        }

        public override PortConnectionData GetPortConnectionDataFromSelf(int nodeId, int portId)
        {
            throw new NotImplementedException();
        }

        public override int GetPortConnectionDataFromSelfCount()
        {
            throw new NotImplementedException();
        }
    }
}