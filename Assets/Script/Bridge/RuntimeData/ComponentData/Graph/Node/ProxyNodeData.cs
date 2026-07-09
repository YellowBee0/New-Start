using System;
using System.Collections.Generic;
using UnityEngine;
using YBFramework.Component;

namespace YBFramework.Bridge
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
    }
}