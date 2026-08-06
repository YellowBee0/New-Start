#if UNITY_EDITOR
using System;
using UnityEngine;
using YBFramework.GameLogic.Graph;

namespace YBFramework.Bridge.Data
{
    [Serializable]
    public sealed class ProxyHelperPortData : BasePortData
    {
        public string ProxyName;

        /// <summary>
        /// 代理端口的索引，同时也是连线数据
        /// </summary>
        [SerializeField] private PortConnectionData m_ProxyPortIndex;

        private BasePortData m_TargetPortData;

        public PortConnectionData GetProxyPortIndex()
        {
            return m_ProxyPortIndex;
        }

        public BasePortData GetProxyPortData()
        {
            return m_TargetPortData;
        }

        public void SetTargetPortData(BasePortData targetPortData)
        {
            m_TargetPortData = targetPortData;
        }

        public override BasePort CreateRuntimeInstance()
        {
            Debug.Log("Editor only port:proxy helper port is tried to create a runtime port");
            return null;
        }

        public override bool Iterator(int index, out PortConnectionData current)
        {
            if (index == 0)
            {
                current = m_ProxyPortIndex;
                return true;
            }
            current = null;
            return false;
        }

        public override PortConnectionData GetPortConnectionDataFromSelf(int nodeId, int portId)
        {
            if (m_ProxyPortIndex.NodeID == nodeId && m_ProxyPortIndex.PortID == portId)
            {
                return m_ProxyPortIndex;
            }
            return null;
        }

        public override int GetPortConnectionDataCountFromSelf()
        {
            if (m_ProxyPortIndex.NodeID != 0 && m_ProxyPortIndex.PortID != 0)
            {
                return 1;
            }
            return 0;
        }

        public override bool CanConnect(BasePortData other)
        {
            return base.CanConnect(other) && other is not ProxyHelperPortData;
        }

        public override void Connect(BasePortData other)
        {
            base.Connect(other);
            m_ProxyPortIndex.NodeID = other.GetNodeData().NodeID;
            m_ProxyPortIndex.PortID = other.PortID;
        }

        public override void Disconnect(BasePortData other)
        {
            base.Disconnect(other);
            if (m_ProxyPortIndex.NodeID == other.GetNodeData().NodeID && m_ProxyPortIndex.PortID == other.PortID)
            {
                m_ProxyPortIndex.NodeID = 0;
                m_ProxyPortIndex.PortID = 0;
            }
            if (GetPortConnectionDataCount() == 0)
            {
                IsUsed = false;
            }
        }

        public override BasePortData Clone()
        {
            throw new Exception("this port can not clone for proxy port");
        }
    }
}
#endif