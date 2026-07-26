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
        [SerializeReference] public BasePortData ClonedTargetPortData;

        public int TargetNodeID;

        /// <summary>
        /// 运行时递归获取重写的端口数据，用于覆盖代理蓝图端口的内部值
        /// </summary>
        /// <returns>重写的端口数据</returns>
        public BasePortData GetRecursionClonedTargetPortData()
        {
            if (ClonedTargetPortData is ProxyPortData proxyPortData)
            {
                return proxyPortData.GetRecursionClonedTargetPortData();
            }
            return ClonedTargetPortData;
        }

        public override BasePort CreateRuntimeInstance()
        {
            ProxyPort proxyPort = new();
            return proxyPort;
        }

        public override bool Iterator(int index, out PortConnectionData current)
        {
            if (ClonedTargetPortData != null)
            {
                return ClonedTargetPortData.Iterator(index, out current);
            }
            current = null;
            return false;
        }
#if UNITY_EDITOR
        //节点初始化的时候调用
        public void SetProxyTargetPortData(ProxyHelperPortData proxyHelperPortData)
        {
            ClonedTargetPortData.MergeData(proxyHelperPortData.GetTargetPortData());
            ClonedTargetPortData.SetFiledName(nameof(ClonedTargetPortData));
            ClonedTargetPortData.SetPortName(string.IsNullOrEmpty(proxyHelperPortData.ProxyName) ? proxyHelperPortData.GetTargetPortData().GetPortName() : proxyHelperPortData.ProxyName);
            ClonedTargetPortData.SetNodeData(m_NodeData);
        }

        public override void SetPortName(string portName)
        {
            ClonedTargetPortData.SetPortName(portName);
        }

        public override void SetDirection(Direction direction)
        {
            ClonedTargetPortData.SetDirection(direction);
        }

        public override void SetCapacity(Port.Capacity capacity)
        {
            ClonedTargetPortData.SetCapacity(capacity);
        }

        public override void SetPortColor(Color portColor)
        {
            ClonedTargetPortData.SetPortColor(portColor);
        }

        public override PortConnectionData GetPortConnectionDataFromSelf(int nodeId, int portId)
        {
            return ClonedTargetPortData?.GetPortConnectionDataFromSelf(nodeId, portId);
        }

        public override int GetPortConnectionDataCountFromSelf()
        {
            return ClonedTargetPortData?.GetPortConnectionDataCountFromSelf() ?? 0;
        }

        public override BasePortData Clone()
        {
            ProxyPortData proxyPortData = new()
            {
                ClonedTargetPortData = ClonedTargetPortData.Clone(),
                TargetNodeID = m_NodeData.NodeID
            };
            return proxyPortData;
        }
#endif
    }
}