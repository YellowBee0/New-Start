using System;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using YBFramework.GameLogic.Graph;
#if UNITY_EDITOR
using UnityEngine.UIElements;
using YBFramework.Bridge.Editor;
#endif

namespace YBFramework.Bridge.Data
{
    [Serializable]
    public sealed class ProxyPortData : BasePortData
    {
        [SerializeReference] private BasePortData m_ProxyPortData;

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
        //TODO:设置这个名字
        private string m_ProxyTargetPortName;

        private BasePortData m_ProxyTargetPortData;

        //节点初始化的时候调用
        public void SetProxyTargetPortData(BasePortData proxyTargetPortDat)
        {
            m_ProxyTargetPortData = proxyTargetPortDat;
        }

        public override PortViewArgs GetPortViewArgs()
        {
            return m_ProxyTargetPortData.GetPortViewArgs();
        }

        public override void SetPortViewArgs(string name, Direction direction, Port.Capacity capacity, Color color)
        {
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

        public override VisualElement CreatePortContentView(out PortView portView)
        {
            VisualElement portContentView = m_ProxyTargetPortData.CreatePortContentView(out portView);
            portView.portName = m_ProxyTargetPortName;
            return portContentView;
        }
#endif
    }
}