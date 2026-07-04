#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEngine;
using YBFramework.Component;

namespace YBFramework.Bridge
{
    [Serializable]
    public sealed class BridgePortData : BasePortData
    {
        [SerializeField] private List<PortConnectionData> m_PortConnectionData;

        public override BasePort CreateRuntimeInstance()
        {
            return null;
        }

        public override PortConnectionData GetPortConnectionDataFromSelf(ushort nodeId, ushort portId)
        {
            if (m_PortConnectionData != null)
            {
                for (int i = 0; i < m_PortConnectionData.Count; i++)
                {
                    PortConnectionData portConnectionData = m_PortConnectionData[i];
                    if (portConnectionData.NodeID == nodeId && portConnectionData.PortID == portId)
                    {
                        return m_PortConnectionData[i];
                    }
                }
            }
            return null;
        }

        public override int GetPortConnectionDataFromSelfCount()
        {
            return m_PortConnectionData?.Count ?? 0;
        }
    }
}
#endif