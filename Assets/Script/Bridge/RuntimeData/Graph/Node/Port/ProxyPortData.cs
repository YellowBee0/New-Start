using System;
using UnityEngine;
using YBFramework.GameLogic.Graph;
#if UNITY_EDITOR
using UnityEditor.Experimental.GraphView;
#endif

namespace YBFramework.Bridge.Data
{
    /// <summary>
    /// 这个端口用于代理其他端口
    /// 这个端口的所有Get获取的数据都是自身数据而不是代理端口的数据，比如GetPortName、GetDirection等。需要注意区分
    /// </summary>
    [Serializable]
    public sealed class ProxyPortData : BasePortData
    {
        [SerializeReference] public BasePortData m_ProxyPortData;

        [SerializeField] private PortConnectionData m_ProxyTargetPortAddress;

        public BasePortData GetProxyPortData()
        {
            if (m_ProxyPortData is ProxyPortData proxyPortData)
            {
                return proxyPortData.GetProxyPortData();
            }
            return m_ProxyPortData;
        }

        public PortConnectionData GetProxyTargetPortAddress()
        {
            return m_ProxyTargetPortAddress;
        }

        public override BasePort CreateRuntimeInstance()
        {
            ProxyPort proxyPort = new();
            return proxyPort;
        }

        public override bool Iterator(int index, out PortConnectionData current)
        {
            if (m_ProxyPortData != null)
            {
                return m_ProxyPortData.Iterator(index, out current);
            }
            current = null;
            return false;
        }
#if UNITY_EDITOR
        //节点初始化的时候调用
        public void SetProxyTargetPortData(BasePortData proxyTargetPortDat, string name)
        {
            m_ProxyPortData.MergeData(proxyTargetPortDat);
            m_ProxyPortData.SetPortName(name);
            m_ProxyPortData.SetNodeData(m_NodeData);
        }

        public override void SetPortName(string portName)
        {
            m_ProxyPortData.SetPortName(portName);
        }

        public override void SetDirection(Direction direction)
        {
            m_ProxyPortData.SetDirection(direction);
        }

        public override void SetCapacity(Port.Capacity capacity)
        {
            m_ProxyPortData.SetCapacity(capacity);
        }

        public override void SetPortColor(Color portColor)
        {
            m_ProxyPortData.SetPortColor(portColor);
        }

        public override PortConnectionData GetPortConnectionDataFromSelf(int nodeId, int portId)
        {
            return m_ProxyPortData?.GetPortConnectionDataFromSelf(nodeId, portId);
        }

        public override int GetPortConnectionDataCountFromSelf()
        {
            return m_ProxyPortData?.GetPortConnectionDataCountFromSelf() ?? 0;
        }

        public override BasePortData Clone()
        {
            ProxyPortData proxyPortData = new()
            {
                m_ProxyPortData = m_ProxyPortData.Clone(),
                m_ProxyTargetPortAddress = new PortConnectionData
                {
                    NodeID = m_NodeData.NodeID,
                    PortID = PortID
                }
            };
            return proxyPortData;
        }
#endif
    }
}