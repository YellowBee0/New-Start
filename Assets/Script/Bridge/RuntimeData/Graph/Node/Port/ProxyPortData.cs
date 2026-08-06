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
        [SerializeReference] private BasePortData m_ClonedProxyPortData;

        [SerializeField] private int m_ProxyNodeID;

        public BasePortData GetClonedProxyPortData()
        {
            return m_ClonedProxyPortData;
        }
        
        public override BasePort CreateRuntimeInstance()
        {
            ProxyPort proxyPort = new();
            return proxyPort;
        }

        public override bool Iterator(int index, out PortConnectionData current)
        {
            if (m_ClonedProxyPortData != null)
            {
                return m_ClonedProxyPortData.Iterator(index, out current);
            }
            current = null;
            return false;
        }
#if UNITY_EDITOR
        public int GetProxyNodeID()
        {
            return m_ProxyNodeID;
        }
        
        public void CloneProxyPortDataFromProxyHelperPortData(ProxyHelperPortData proxyHelperPortData)
        {
            m_ClonedProxyPortData = proxyHelperPortData.GetProxyPortData().Clone();
            m_ProxyNodeID = proxyHelperPortData.GetProxyPortIndex().NodeID;
        }

        public override void SetFiledName(string filedName)
        {
            base.SetFiledName(filedName);
            m_ClonedProxyPortData.SetFiledName(nameof(m_ClonedProxyPortData));
        }

        public override void SetPortName(string portName)
        {
            base.SetPortName(portName);
            m_ClonedProxyPortData.SetPortName(portName);
        }

        public override void SetDirection(Direction direction)
        {
            Debug.LogWarning($"It's useless to set proxy port's port direction: {direction}, because the proxy port's direction is limited by the proxy port's direction");
        }

        public override void SetCapacity(Port.Capacity capacity)
        {
            Debug.LogWarning($"It's useless to set proxy port's port capacity: {capacity}, because the proxy port's capacity is limited by the proxy port's capacity");
        }

        public override void SetPortColor(Color portColor)
        {
            Debug.LogWarning($"It's useless to set proxy port's port color: {portColor}, because the proxy port's color is limited by the proxy port's color");
        }

        public override PortConnectionData GetPortConnectionDataFromSelf(int nodeId, int portId)
        {
            return m_ClonedProxyPortData.GetPortConnectionDataFromSelf(nodeId, portId);
        }

        public override int GetPortConnectionDataCountFromSelf()
        {
            return m_ClonedProxyPortData.GetPortConnectionDataCountFromSelf();
        }

        public override bool CanConnect(BasePortData other)
        {
            return m_ClonedProxyPortData.CanConnect(other);
        }

        public override BasePortData Clone()
        {
            ProxyPortData proxyPortData = new()
            {
                m_ClonedProxyPortData = m_ClonedProxyPortData.Clone(),
                m_ProxyNodeID = m_NodeData.NodeID
            };
            return proxyPortData;
        }

        public override void MergeData(BasePortData dataToMerge)
        {
            ProxyPortData proxyPortDataToMerge = (ProxyPortData)dataToMerge;
            m_ClonedProxyPortData.MergeData(proxyPortDataToMerge.m_ClonedProxyPortData);
            m_Direction = m_ClonedProxyPortData.GetDirection();
            m_Capacity = m_ClonedProxyPortData.GetCapacity();
            m_PortColor = m_ClonedProxyPortData.GetPortColor();
        }
#endif
    }
}