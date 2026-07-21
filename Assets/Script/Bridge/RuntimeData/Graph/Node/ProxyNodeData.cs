using System;
using System.Collections.Generic;
using UnityEngine;
using YBFramework.GameLogic.Graph;
#if UNITY_EDITOR
using YBFramework.Bridge.Editor;
#endif

namespace YBFramework.Bridge.Data
{
    [Serializable]
#if UNITY_EDITOR
    [NodeMenu("蓝图代理", GraphType.Everything)]
#endif
    public sealed class ProxyNodeData : BaseNodeData
    {
        [SerializeField] public List<ProxyPortData> ProxyPortData;

        [SerializeField] public GraphAsset ProxyGraphAsset;

        public GraphAsset GetGraphAsset()
        {
            return ProxyGraphAsset;
        }

        public IReadOnlyList<ProxyPortData> GetProxyPortData()
        {
            return ProxyPortData;
        }

        public override BaseNode CreateRuntimeInstance()
        {
            ProxyNode node = new();
            node.InitializeFromProxyNodeData(this);
            return node;
        }

        public override bool Iterator(int index, out BasePortData current)
        {
            if (ProxyPortData != null && index < ProxyPortData.Count)
            {
                current = ProxyPortData[index];
                return true;
            }
            current = null;
            return false;
        }
#if UNITY_EDITOR
        public override void Initialize()
        {
            if (ProxyGraphAsset == null)
            {
                return;
            }
            //确保蓝图中节点全部初始化
            ProxyGraphAsset.Initialize();

            IReadOnlyList<BaseNodeData> nodeData = ProxyGraphAsset.GetNodeData();
            for (int i = 0; i < nodeData.Count; i++)
            {
                if (nodeData[i] is ProxyTargetNodeData proxyTargetNodeData)
                {
                }
            }

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