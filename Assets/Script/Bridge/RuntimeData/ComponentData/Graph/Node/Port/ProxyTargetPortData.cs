#if UNITY_EDITOR
using System;
using UnityEngine;
using YBFramework.Component;

namespace YBFramework.Bridge
{
    [Serializable]
    public sealed class ProxyTargetPortData : BasePortData
    {
        [SerializeField] private PortConnectionData m_PortConnectionData;

        public override BasePort CreateRuntimeInstance()
        {
            return null;
        }

        public override PortConnectionData GetPortConnectionDataFromSelf(int nodeId, int portId)
        {
            if (m_PortConnectionData != null)
            {
                if (m_PortConnectionData.NodeID == nodeId && m_PortConnectionData.PortID == portId)
                {
                    return m_PortConnectionData;
                }
            }
            return null;
        }

        public override int GetPortConnectionDataFromSelfCount()
        {
            if (m_PortConnectionData != null && m_PortConnectionData.NodeID != 0 && m_PortConnectionData.PortID != 0)
            {
                return 1;
            }
            return 0;
        }
    }
}
#endif