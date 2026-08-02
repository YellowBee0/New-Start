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
            if (index < m_ProxyPortsData.Count)
            {
                current = m_ProxyPortsData[index];
                return true;
            }
            current = null;
            return false;
        }
#if UNITY_EDITOR
        private const string LIST_DATA_PATH = nameof(m_ProxyPortsData) + ".Array.data[{0}]";

        public int PortID;

        private ProxyPortData InitializeSerializedProxyPortData(ProxyHelperPortData proxyHelperPortData)
        {
            ProxyPortData proxyPortData = CreatePortData<ProxyPortData>(++PortID);
            proxyPortData.ClonedTargetPortData = proxyHelperPortData.GetTargetPortData().Clone();
            proxyPortData.TargetNodeID = proxyHelperPortData.TargetPortConnectionData.NodeID;
            m_ProxyPortsData.Add(proxyPortData);
            return proxyPortData;
        }

        private void InitializeProxyPortData(ProxyPortData proxyPortData, ProxyHelperPortData proxyHelperPortData, int index)
        {
            proxyPortData.SetNodeData(this);
            proxyPortData.SetProxyTargetPortData(proxyHelperPortData);
            proxyPortData.SetFiledName(string.Format(LIST_DATA_PATH, index));
        }

        public void ChangeProxyGraphAsset(GraphAsset proxyGraphAsset)
        {
            m_ProxyPortsData.Clear();
            //确保初始化
            proxyGraphAsset.Initialize();
            if (proxyGraphAsset != null)
            {
                IReadOnlyList<BaseNodeData> nodeData = proxyGraphAsset.GetNodesData();
                for (int i = 0; i < nodeData.Count; i++)
                {
                    if (nodeData[i] is ProxyHelperNodeData proxyHelperNodeData)
                    {
                        for (int j = 0; j < proxyHelperNodeData.ProxyHelperPortsData.Count; j++)
                        {
                            ProxyHelperPortData proxyHelperPortData = proxyHelperNodeData.ProxyHelperPortsData[j];
                            if (proxyHelperPortData.TargetPortConnectionData.NodeID == 0 || proxyHelperPortData.TargetPortConnectionData.PortID == 0)
                            {
                                Debug.LogWarning($"Port in graph:{ProxyGraphAsset.name} node id:{proxyHelperNodeData.NodeID} port id:{proxyHelperPortData.PortID} did not connect any other port");
                                continue;
                            }
                            ProxyPortData proxyPortData = InitializeSerializedProxyPortData(proxyHelperPortData);
                            InitializeProxyPortData(proxyPortData, proxyHelperPortData, j);
                            //TODO:这里创建端口并添加到集合并不会更新数据到SO，导致获取不到序列化对象。但是这里也获取不到SO。
                            // 目前想的解决办法是不直接更新数据，而是显示有问题的数据，比如哪一个被移除，哪一个是新增。
                            // 让用户能够知道改变，然后只给出一个更新按钮
                        }
                        //创建端口步骤：1、创建端口实例，调用CreatePortData，设置port id，创建需要序列化的数据
                        //2、设置NodeData
                        //3、初始化端口非序列化数据（端口朝向、容量、颜色、名字、字段名字。。。）
                    }
                }
            }
            ProxyGraphAsset = proxyGraphAsset;
        }

        public void MergeProxyTargetNodeData()
        {
            IReadOnlyList<BaseNodeData> nodeData = ProxyGraphAsset.GetNodesData();
            for (int i = 0; i < nodeData.Count; i++)
            {
                if (nodeData[i] is ProxyHelperNodeData proxyTargetNodeData)
                {
                    for (int j = 0; j < proxyTargetNodeData.ProxyHelperPortsData.Count; j++)
                    {
                        ProxyHelperPortData proxyHelperPortData = proxyTargetNodeData.ProxyHelperPortsData[j];
                        //insideNodeData和insidePortData一般都不为null，只有在数据非法修改后才可能为null，所以这里就不做判断
                        if (proxyHelperPortData.TargetPortConnectionData.NodeID == 0 || proxyHelperPortData.TargetPortConnectionData.PortID == 0)
                        {
                            Debug.LogWarning($"Port in graph:{ProxyGraphAsset.name} node id:{proxyTargetNodeData.NodeID} port id:{proxyHelperPortData.PortID} did not connect any other port");
                            continue;
                        }
                        ProxyPortData proxyPortData = GetProxyPortData(proxyHelperPortData.TargetPortConnectionData.NodeID, proxyHelperPortData.TargetPortConnectionData.PortID);
                        //TODO:这里只做了少了会添加，但是多了不会删除。需要取两个集合的交集
                        if (proxyPortData == null)
                        {
                            Debug.Log("Proxy node didn't save proxy port data for port address:" +
                                      $"node id:{proxyHelperPortData.TargetPortConnectionData.NodeID} port id:{proxyHelperPortData.TargetPortConnectionData.PortID},this will create a new one");
                            proxyPortData = new ProxyPortData
                            {
                                ClonedTargetPortData = proxyHelperPortData.GetTargetPortData().Clone(),
                                TargetNodeID = proxyHelperPortData.TargetPortConnectionData.NodeID
                            };
                            //TODO:这里创建端口并添加到集合并不会更新数据到SO，导致获取不到序列化对象。但是这里也获取不到SO。
                            // 目前想的解决办法是不直接更新数据，而是显示有问题的数据，比如哪一个被移除，哪一个是新增。
                            // 让用户能够知道改变，然后只给出一个更新按钮
                            m_ProxyPortsData.Add(proxyPortData);
                        }
                        proxyPortData.SetProxyTargetPortData(proxyHelperPortData);
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
            base.Initialize();
            //确保蓝图中节点全部初始化
            ProxyGraphAsset.Initialize();
            MergeProxyTargetNodeData();
            for (int i = 0; i < m_ProxyPortsData.Count; i++)
            {
                m_ProxyPortsData[i].SetFiledName(string.Format(LIST_DATA_PATH, i));
            }
            //TODO:在这里查找蓝图中ProxyTargetNodeData，获取最新的数据，需要对比数据是否有改变，有改变需要针对改变的对象进行删除或者重新Clone
        }

        private ProxyPortData GetProxyPortData(int nodeID, int portID)
        {
            for (int i = 0; i < m_ProxyPortsData.Count; i++)
            {
                ProxyPortData proxyPortData = m_ProxyPortsData[i];
                if (proxyPortData.TargetNodeID == nodeID && proxyPortData.ClonedTargetPortData.PortID == portID)
                {
                    return proxyPortData;
                }
            }
            return null;
        }
#endif
    }
}