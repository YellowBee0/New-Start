using System;
using System.Collections.Generic;
using UnityEngine;
using YBFramework.GameLogic.Graph;

namespace YBFramework.Bridge.Data
{
    [Serializable]
    public sealed class ProxyNodeData : BaseNodeData
    {
        [SerializeField] private List<ProxyPortData> m_ProxyPortData;

        [SerializeField] private GraphAsset m_GraphAsset;

        public GraphAsset GetGraphAsset()
        {
            return m_GraphAsset;
        }

        public IReadOnlyList<ProxyPortData> GetProxyPortData()
        {
            return m_ProxyPortData;
        }

        public override BaseNode CreateRuntimeInstance()
        {
            ProxyNode node = new();
            node.InitializeFromProxyNodeData(this);
            return node;
        }

        public override bool Iterator(int index, out BasePortData current)
        {
            if (m_ProxyPortData != null && index < m_ProxyPortData.Count)
            {
                current = m_ProxyPortData[index];
                return true;
            }
            current = null;
            return false;
        }
#if UNITY_EDITOR
        public override void Initialize()
        {
            //确保蓝图中节点全部初始化
            m_GraphAsset.Initialize();
            //TODO:在这里查找蓝图中ProxyTargetNodeData，获取最新的数据，需要对比数据是否有改变，有改变需要针对改变的对象进行删除或者重新Clone
            /*foreach (ProxyPortData proxyPortData in m_ProxyPortData)
            {
                PortConnectionData portAddress = proxyPortData.GetProxyTargetPortAddress();
                BaseNodeData nodeData = m_GraphAsset.GetNodeData(portAddress.NodeID);
                if (nodeData == null)
                {
                    Debug.LogWarning($"Graph {m_GraphAsset.name} was missing node {portAddress.NodeID}");
                    continue;
                }
                BasePortData portData = nodeData.GetPortData(portAddress.PortID);
                if (portData == null)
                {
                    Debug.LogWarning($"Node {portAddress.NodeID} in graph {m_GraphAsset.name} was missing port {portAddress.PortID}");
                    continue;
                }
                proxyPortData.SetProxyTargetPortData(portData);
            }*/
        }
#endif
    }
}