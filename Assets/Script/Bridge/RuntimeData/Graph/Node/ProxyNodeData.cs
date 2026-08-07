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

        [SerializeField] private GraphAsset m_ProxyGraphAsset;

        public IReadOnlyList<ProxyPortData> GetProxyPortsData()
        {
            return m_ProxyPortsData;
        }

        public GraphAsset GetProxyGraphAsset()
        {
            return m_ProxyGraphAsset;
        }

        public override BaseNode CreateRuntimeInstance()
        {
            ProxyNode node = new();
            node.InitializeFromProxyNodeData(this);
            return node;
        }

        public override bool Iterator(int index, out BasePortData current)
        {
            if (index < m_ProxyPortsData.Count)
            {
                current = m_ProxyPortsData[index];
                return true;
            }
            current = null;
            return false;
        }
#if UNITY_EDITOR
        public void SetProxyGraphAsset(GraphAsset proxyGraphAsset)
        {
            //断开所有连接，清空数据
            for (int i = 0; i < m_ProxyPortsData.Count; i++)
            {
                m_ProxyPortsData[i].DisconnectAll();
            }
            m_ProxyPortsData.Clear();
            //确保初始化
            proxyGraphAsset.InitializeNodeData();
            if (proxyGraphAsset != null)
            {
                IReadOnlyList<BaseNodeData> nodeData = proxyGraphAsset.GetNodesData();
                for (int i = 0; i < nodeData.Count; i++)
                {
                    if (nodeData[i] is ProxyHelperNodeData proxyHelperNodeData)
                    {
                        IReadOnlyList<ProxyHelperPortData> proxyHelperPortsData = proxyHelperNodeData.GetProxyHelperPortsData();
                        for (int j = 0; j < proxyHelperPortsData.Count; j++)
                        {
                            ProxyHelperPortData proxyHelperPortData = proxyHelperPortsData[j];
                            if (proxyHelperPortData.GetProxyPortIndex().NodeID == 0 || proxyHelperPortData.GetProxyPortIndex().PortID == 0)
                            {
                                Debug.LogWarning($"Port in graph:{m_ProxyGraphAsset.name} node id:{proxyHelperNodeData.NodeID} port id:{proxyHelperPortData.PortID} did not connect any other port");
                                continue;
                            }
                            CloneProxyPortDataFromProxyHelperPortData(proxyHelperPortData);
                        }
                    }
                }
            }
            m_ProxyGraphAsset = proxyGraphAsset;
        }

        public bool MigrateProxyHelperNodeData(ProxyHelperNodeData proxyHelperNodeData)
        {
            bool isMigrated = false;
            IReadOnlyList<ProxyHelperPortData> proxyHelperPortsData = proxyHelperNodeData.GetProxyHelperPortsData();
            for (int i = 0; i < proxyHelperPortsData.Count; i++)
            {
                ProxyHelperPortData proxyHelperPortData = proxyHelperPortsData[i];
                int targetNodeID = proxyHelperPortData.GetProxyPortIndex().NodeID;
                int targetPortID = proxyHelperPortData.GetProxyPortIndex().PortID;
                if (targetNodeID == 0 || targetPortID == 0)
                {
                    //这个换到上一级调用时判断
                    Debug.LogWarning($"Port in graph:{m_ProxyGraphAsset.name} node id:{proxyHelperNodeData.NodeID} port id:{proxyHelperPortData.PortID} did not connect any other port");
                    continue;
                }
                bool isNotFound = true;
                for (int k = 0; k < m_ProxyPortsData.Count; k++)
                {
                    ProxyPortData proxyPortData = m_ProxyPortsData[k];
                    if (proxyPortData.GetProxyNodeID() == targetNodeID && proxyPortData.GetClonedProxyPortData().PortID == targetPortID)
                    {
                        isNotFound = false;
                        break;
                    }
                }
                if (isNotFound)
                {
                    //TODO:这里Clone有一步获取内部代理端口并没有赋初始值，导致不能找到
                    CloneProxyPortDataFromProxyHelperPortData(proxyHelperPortData);
                    isMigrated = true;
                }
            }
            //TODO:遍历查看哪一个端口这里有，但是代理辅助没有。找到后删除数据和连线，并设置isMigrated = true
            return isMigrated;
        }

        public override void InitializeSerializedData()
        {
            m_ProxyPortsData = new List<ProxyPortData>();
        }

        public override void Initialize()
        {
            if (m_ProxyGraphAsset == null)
            {
                return;
            }
            //确保蓝图中节点全部初始化
            m_ProxyGraphAsset.InitializeNodeData();
            IReadOnlyList<BaseNodeData> nodeData = m_ProxyGraphAsset.GetNodesData();
            for (int i = 0; i < nodeData.Count; i++)
            {
                if (nodeData[i] is ProxyHelperNodeData proxyHelperNodeData)
                {
                    //TODO:需要处理代理节点删除或者新增
                    IReadOnlyList<ProxyHelperPortData> proxyHelperPortsData = proxyHelperNodeData.GetProxyHelperPortsData();
                    for (int j = 0; j < proxyHelperPortsData.Count; j++)
                    {
                        ProxyHelperPortData proxyHelperPortData = proxyHelperPortsData[j];
                        int targetNodeID = proxyHelperPortData.GetProxyPortIndex().NodeID;
                        int targetPortID = proxyHelperPortData.GetProxyPortIndex().PortID;
                        if (targetNodeID == 0 || targetPortID == 0)
                        {
                            Debug.LogWarning($"Port in graph:{m_ProxyGraphAsset.name} node id:{proxyHelperNodeData.NodeID} port id:{proxyHelperPortData.PortID} did not connect any other port");
                            continue;
                        }
                        bool isNotFound = true;
                        for (int k = 0; k < m_ProxyPortsData.Count; k++)
                        {
                            ProxyPortData proxyPortData = m_ProxyPortsData[k];
                            if (proxyPortData.GetProxyNodeID() == targetNodeID && proxyPortData.GetClonedProxyPortData().PortID == targetPortID)
                            {
                                InitializeProxyPortData(proxyPortData, proxyHelperPortData, k);
                                isNotFound = false;
                                break;
                            }
                        }
                        if (isNotFound)
                        {
                            CloneProxyPortDataFromProxyHelperPortData(proxyHelperPortData);
                        }
                    }
                    //查找到两次就break
                }
            }
        }

        private void CloneProxyPortDataFromProxyHelperPortData(ProxyHelperPortData proxyHelperPortData)
        {
            ProxyPortData proxyPortData = CreatePortData<ProxyPortData>();
            proxyPortData.PortID = GetNextPortID();
            proxyPortData.CloneProxyPortDataFromProxyHelperPortData(proxyHelperPortData);
            InitializeProxyPortData(proxyPortData, proxyHelperPortData, m_ProxyPortsData.Count);
            m_ProxyPortsData.Add(proxyPortData);
        }

        private void InitializeProxyPortData(ProxyPortData proxyPortData, ProxyHelperPortData proxyHelperPortData, int index)
        {
            proxyPortData.SetNodeData(this);
            proxyPortData.SetFiledName($"{nameof(m_ProxyPortsData)}.Array.data[{index}]");
            proxyPortData.SetPortName(string.IsNullOrEmpty(proxyHelperPortData.ProxyName) ? proxyHelperPortData.GetProxyPortData().GetPortName() : proxyHelperPortData.ProxyName);
            proxyPortData.MergeData(proxyHelperPortData.GetProxyPortData());
        }
#endif
    }
}