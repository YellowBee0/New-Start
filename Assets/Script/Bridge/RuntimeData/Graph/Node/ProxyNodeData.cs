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
    }
}