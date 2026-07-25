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
        [SerializeReference] public BasePortData ProxyTargetClonedPortData;

        public PortConnectionData ProxyTargetPortAddress;

        public BasePortData GetProxyPortData()
        {
            if (ProxyTargetClonedPortData is ProxyPortData proxyPortData)
            {
                return proxyPortData.GetProxyPortData();
            }
            return ProxyTargetClonedPortData;
        }

        public PortConnectionData GetProxyTargetPortAddress()
        {
            return ProxyTargetPortAddress;
        }

        public override BasePort CreateRuntimeInstance()
        {
            ProxyPort proxyPort = new();
            return proxyPort;
        }

        public override bool Iterator(int index, out PortConnectionData current)
        {
            if (ProxyTargetClonedPortData != null)
            {
                return ProxyTargetClonedPortData.Iterator(index, out current);
            }
            current = null;
            return false;
        }
#if UNITY_EDITOR
        //节点初始化的时候调用
        public void SetProxyTargetPortData(BasePortData proxyTargetPortDat, string name)
        {
            ProxyTargetClonedPortData.MergeData(proxyTargetPortDat);
            ProxyTargetClonedPortData.SetPortName(name);
            ProxyTargetClonedPortData.NodeData = NodeData;
        }

        public override void SetPortName(string portName)
        {
            ProxyTargetClonedPortData.SetPortName(portName);
        }

        public override void SetDirection(Direction direction)
        {
            ProxyTargetClonedPortData.SetDirection(direction);
        }

        public override void SetCapacity(Port.Capacity capacity)
        {
            ProxyTargetClonedPortData.SetCapacity(capacity);
        }

        public override void SetPortColor(Color portColor)
        {
            ProxyTargetClonedPortData.SetPortColor(portColor);
        }

        public override PortConnectionData GetPortConnectionDataFromSelf(int nodeId, int portId)
        {
            return ProxyTargetClonedPortData?.GetPortConnectionDataFromSelf(nodeId, portId);
        }

        public override int GetPortConnectionDataCountFromSelf()
        {
            return ProxyTargetClonedPortData?.GetPortConnectionDataCountFromSelf() ?? 0;
        }

        public override BasePortData Clone()
        {
            ProxyPortData proxyPortData = new()
            {
                ProxyTargetClonedPortData = ProxyTargetClonedPortData.Clone(),
                ProxyTargetPortAddress = new PortConnectionData
                {
                    NodeID = NodeData.NodeID,
                    PortID = PortID
                }
            };
            return proxyPortData;
        }
#endif
    }
}