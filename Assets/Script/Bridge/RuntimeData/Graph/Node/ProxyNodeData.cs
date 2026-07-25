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
        [SerializeField] private List<ProxyPortData> m_ProxyPortsData;

        public GraphAsset ProxyGraphAsset;

        public IReadOnlyList<ProxyPortData> GetProxyPortsData()
        {
            return m_ProxyPortsData;
        }

        public override BaseNode CreateRuntimeInstance()
        {
            ProxyNode node = new();
            node.InitializeFromProxyNodeData(this);
            return node;
        }

        public override bool Iterator(int index, out BasePortData current)
        {
            if (m_ProxyPortsData != null && index < m_ProxyPortsData.Count)
            {
                current = m_ProxyPortsData[index];
                return true;
            }
            current = null;
            return false;
        }
#if UNITY_EDITOR
        public void MergeProxyTargetNodeData()
        {
            IReadOnlyList<BaseNodeData> nodeData = ProxyGraphAsset.GetNodeData();
            for (int i = 0; i < nodeData.Count; i++)
            {
                if (nodeData[i] is ProxyTargetNodeData proxyTargetNodeData)
                {
                    for (int j = 0; j < proxyTargetNodeData.ProxyTargetPortsData.Count; j++)
                    {
                        ProxyTargetPortData proxyTargetPortData = proxyTargetNodeData.ProxyTargetPortsData[j];
                        PortConnectionData insideProxyTargetPortAddress = proxyTargetPortData.PortConnectionData;
                        //insideNodeData和insidePortData一般都不为null，只有在数据非法修改后才可能为null，所以这里就不做判断
                        BaseNodeData insideNodeData = ProxyGraphAsset.GetNodeData(insideProxyTargetPortAddress.NodeID);
                        BasePortData insideProxyTargetPortData = insideNodeData.GetPortData(insideProxyTargetPortAddress.PortID);
                        ProxyPortData proxyPortData = GetProxyPortData(insideProxyTargetPortAddress.NodeID, insideProxyTargetPortData.PortID);
                        //TODO:这里只做了少了会添加，但是多了不会删除。需要取两个集合的交集
                        if (proxyPortData == null)
                        {
                            Debug.Log($"Proxy node didn't save proxy port data for port address:{insideProxyTargetPortAddress.NodeID}{insideProxyTargetPortData.PortID},this will create a new one");
                            proxyPortData = new ProxyPortData
                            {
                                ProxyTargetClonedPortData = insideProxyTargetPortData.Clone(),
                                ProxyTargetPortAddress = new PortConnectionData
                                {
                                    NodeID = insideProxyTargetPortAddress.NodeID,
                                    PortID = insideProxyTargetPortAddress.PortID
                                }
                            };
                            m_ProxyPortsData.Add(proxyPortData);
                        }
                        proxyPortData.SetProxyTargetPortData(insideProxyTargetPortData, proxyTargetPortData.ProxyName);
                    }
                }
            }
        }

        public override void CreateData()
        {
            m_ProxyPortsData = new List<ProxyPortData>();
        }

        public override void Initialize()
        {
            if (ProxyGraphAsset == null)
            {
                return;
            }
            //确保蓝图中节点全部初始化
            ProxyGraphAsset.Initialize();
            MergeProxyTargetNodeData();
            for (int i = 0; i < m_ProxyPortsData.Count; i++)
            {
                m_ProxyPortsData[i].SetFiledName($"{nameof(m_ProxyPortsData)}.Array.data[{i}]");
            }
            //TODO:在这里查找蓝图中ProxyTargetNodeData，获取最新的数据，需要对比数据是否有改变，有改变需要针对改变的对象进行删除或者重新Clone
        }

        private ProxyPortData GetProxyPortData(int nodeID, int portID)
        {
            for (int i = 0; i < m_ProxyPortsData.Count; i++)
            {
                ProxyPortData proxyPortData = m_ProxyPortsData[i];
                PortConnectionData outsideProxyTargetPortAddress = proxyPortData.GetProxyTargetPortAddress();
                if (outsideProxyTargetPortAddress.NodeID == nodeID && outsideProxyTargetPortAddress.PortID == portID)
                {
                    return proxyPortData;
                }
            }
            return null;
        }
#endif
    }
}