using System;
using System.Collections.Generic;
using UnityEngine;
using YBFramework.GameLogic.Graph;
#if UNITY_EDITOR
using Object = UnityEngine.Object;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using YBFramework.Bridge.Editor;
#endif

namespace YBFramework.Bridge.Data
{
    [Serializable]
#if UNITY_EDITOR
    [NodeMenu("代理节点", GraphType.Everything)]
#endif
    public sealed class ProxyNodeData : BaseNodeData
    {
        [SerializeField] private List<ProxyPortData> m_ProxyPortData;

        [SerializeField] private GraphAsset m_ProxyGraphAsset;

        public GraphAsset GetGraphAsset()
        {
            return m_ProxyGraphAsset;
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
            m_ProxyGraphAsset.Initialize();
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

        public override NodeView CreateNodeView()
        {
            NodeView nodeView = base.CreateNodeView();
            ObjectField proxyGraphAssetField = new()
            {
                value = m_ProxyGraphAsset
            };
            proxyGraphAssetField.RegisterValueChangedCallback(OnProxyGraphAssetChanged);
            nodeView.contentContainer.Add(proxyGraphAssetField);
            return nodeView;
        }

        private void OnProxyGraphAssetChanged(ChangeEvent<Object> evt)
        {
            if (evt.newValue is GraphAsset proxyGraphAsset)
            {
                if ((m_GraphAsset.GetGraphType() & proxyGraphAsset.GetGraphType()) == proxyGraphAsset.GetGraphType())
                {
                    //TODO:需要支持Undo
                    m_ProxyGraphAsset = proxyGraphAsset;
                }
                else
                {
                    Debug.LogError($"This graph type:{m_GraphAsset.GetGraphType()} is not contains proxy graph type:{proxyGraphAsset.GetGraphType()}");
                }
            }
            else
            {
                Debug.LogError($"{evt.newValue} is not type of GraphAsset");
            }
        }
#endif
    }
}