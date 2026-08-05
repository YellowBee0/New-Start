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

        private bool m_IsValid;

        /// <summary>
        /// 运行时递归获取重写的端口数据，用于覆盖代理蓝图端口的内部值
        /// </summary>
        /// <returns>重写的端口数据</returns>
        public BasePortData GetRecursionClonedTargetPortData()
        {
            if (m_ClonedProxyPortData is ProxyPortData proxyPortData)
            {
                return proxyPortData.GetRecursionClonedTargetPortData();
            }
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

        public BasePortData GetClonedProxyPortData()
        {
            return m_ClonedProxyPortData;
        }

        public void CloneProxyPortDataFromProxyHelperPortData(ProxyHelperPortData proxyHelperPortData)
        {
            m_ClonedProxyPortData = proxyHelperPortData.GetProxyPortData().Clone();
            m_ProxyNodeID = proxyHelperPortData.GetProxyPortIndex().NodeID;
        }

        public void Initialize(ProxyHelperPortData proxyHelperPortData)
        {
            m_ClonedProxyPortData.SetNodeData(m_NodeData);
            m_ClonedProxyPortData.MergeData(proxyHelperPortData.GetProxyPortData());
            m_ClonedProxyPortData.SetFiledName(nameof(m_ClonedProxyPortData));
            m_ClonedProxyPortData.SetPortName(string.IsNullOrEmpty(proxyHelperPortData.ProxyName) ? proxyHelperPortData.GetProxyPortData().GetPortName() : proxyHelperPortData.ProxyName);
        }

        public override void SetFiledName(string filedName)
        {
            base.SetFiledName(filedName);
            m_ClonedProxyPortData.SetFiledName(nameof(m_ClonedProxyPortData));
        }

        public override string GetPortName()
        {
            return m_ClonedProxyPortData.GetPortName();
        }

        public override void SetPortName(string portName)
        {
            m_ClonedProxyPortData.SetPortName(portName);
        }

        public override Direction GetDirection()
        {
            return m_ClonedProxyPortData.GetDirection();
        }

        public override void SetDirection(Direction direction)
        {
            m_ClonedProxyPortData.SetDirection(direction);
        }

        public override Port.Capacity GetCapacity()
        {
            return m_ClonedProxyPortData.GetCapacity();
        }

        public override void SetCapacity(Port.Capacity capacity)
        {
            m_ClonedProxyPortData.SetCapacity(capacity);
        }

        public override Color GetPortColor()
        {
            return m_ClonedProxyPortData.GetPortColor();
        }

        public override void SetPortColor(Color portColor)
        {
            m_ClonedProxyPortData.SetPortColor(portColor);
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
            return m_IsValid && m_ClonedProxyPortData.CanConnect(other);
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
            if (dataToMerge is ProxyPortData proxyPortData)
            {
                dataToMerge = proxyPortData.GetRecursionClonedTargetPortData();
            }
            m_ClonedProxyPortData.MergeData(dataToMerge);
        }
#endif
    }
}