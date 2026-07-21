using System;
using UnityEngine;
using YBFramework.GameLogic.Graph;
#if UNITY_EDITOR
using UnityEditor.Experimental.GraphView;
using YBFramework.Bridge.Editor;
#endif

namespace YBFramework.Bridge.Data
{
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
            //m_ProxyPortData.SetPortViewArgs(name); 设置端口名
        }

        public override PortViewArgs GetPortViewArgs()
        {
            return m_ProxyPortData.GetPortViewArgs();
        }

        //TODO:
        public override void SetPortViewArgs(string name, Direction direction, Port.Capacity capacity, Color color)
        {
            Debug.LogWarning("It is useless to set proxy port's port view ");
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