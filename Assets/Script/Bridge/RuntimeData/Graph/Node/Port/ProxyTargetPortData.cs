#if UNITY_EDITOR
using System;
using UnityEngine;
using YBFramework.GameLogic.Graph;

namespace YBFramework.Bridge.Data
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

        public override int GetPortConnectionDataCountFromSelf()
        {
            if (m_PortConnectionData != null && m_PortConnectionData.NodeID != 0 && m_PortConnectionData.PortID != 0)
            {
                return 1;
            }
            return 0;
        }

        public override BasePortData Clone()
        {
            throw new Exception("this port can not clone for proxy port");
        }
    }
}
#endif