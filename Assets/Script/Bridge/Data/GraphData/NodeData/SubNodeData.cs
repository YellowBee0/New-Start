using System;
using System.Collections.Generic;
using UnityEngine;
using YBFramework.GameLogic.Graph;
#if UNITY_EDITOR
using System.Buffers;
using YBFramework.Bridge.Editor;
#endif

namespace YBFramework.Bridge.Data
{
    [Serializable]
#if UNITY_EDITOR
    [NodeMenu("子图节点", GraphType.Everything)]
#endif
    public sealed class SubNodeData : BaseNodeData
    {
        [SerializeField] private GraphAsset m_SubGraphAsset;

        [SerializeField] private List<SubPortData> m_SubPortsData;

        public GraphAsset GetSubGraphAsset()
        {
            return m_SubGraphAsset;
        }

        public SubPortData FindSubPortDataBySubPortAddress(int subNodeID, int subPortID)
        {
            int index = FindSubPortDataIndexBySubPortAddress(subNodeID, subPortID);
            return index != -1 ? m_SubPortsData[index] : null;
        }

        private int FindSubPortDataIndexBySubPortAddress(int subNodeID, int subPortID)
        {
            for (int i = 0; i < m_SubPortsData.Count; i++)
            {
                SubPortData subPortData = m_SubPortsData[i];
                if (subPortData.GetAsSubNodeID() == subNodeID && subPortData.GetAsSubPortID() == subPortID)
                {
                    return i;
                }
            }
            return -1;
        }

        public override int GetPortsDataCount()
        {
            return m_SubPortsData.Count;
        }

        public override BasePortData PortDataOfIndex(int index)
        {
            return m_SubPortsData[index];
        }

        public override BaseNode CreateRuntimeInstance(NodeSliceData nodeSliceData)
        {
            /*ProxyNode proxyNode = new ProxyNode();
            proxyNode.InitializeFromProxyNodeData(this, (SubNodeSliceData)nodeSliceData);
            return proxyNode;*/
            throw new NotImplementedException();
        }

        public override void CheckExecutionSliceEntry(DFSGraphAsset dfsGraphAsset)
        {
            GraphSliceData graphSliceData = dfsGraphAsset.GetGraphSliceData();
            if (!graphSliceData.TryGetNodeSliceData(this, out NodeSliceData nodeSliceData))
            {
                nodeSliceData = new SubNodeSliceData(m_SubGraphAsset, new GraphSliceData());
                graphSliceData.AddNodeSliceData(this, nodeSliceData);
            }
            dfsGraphAsset.DFSNodeData = new DFSNodeData(this, nodeSliceData);
            DFSGraphAsset subDFSGraphAsset = DFSGraphAsset.Allocate(m_SubGraphAsset, new GraphSliceData());
            subDFSGraphAsset.SetParent(dfsGraphAsset);
            IReadOnlyList<BaseNodeData> subNodesData = m_SubGraphAsset.GetNodesData();
            for (int i = 0; i < subNodesData.Count; i++)
            {
                subNodesData[i].CheckExecutionSliceEntry(subDFSGraphAsset);
            }
            DFSGraphAsset.Release(subDFSGraphAsset);
        }

        public override void DFSExecutionFlow(DFSGraphAsset dfsGraphAsset, BasePortData portData)
        {
            NodeSliceData nodeSliceData;
            if (dfsGraphAsset.DFSNodeData.NodeData == this)
            {
                nodeSliceData = dfsGraphAsset.DFSNodeData.NodeSliceData;
            }
            else
            {
                GraphSliceData graphSliceData = dfsGraphAsset.GetGraphSliceData();
                if (!graphSliceData.TryGetNodeSliceData(this, out nodeSliceData))
                {
                    nodeSliceData = new SubNodeSliceData(m_SubGraphAsset, new GraphSliceData());
                    graphSliceData.AddNodeSliceData(this, nodeSliceData);
                }
            }
            if (nodeSliceData.AddPortSliceData(portData))
            {
                dfsGraphAsset.DFSNodeData = new DFSNodeData(this, nodeSliceData);
                //执行子端口在当前蓝图调用链的检查
                portData.DFSExecutionFlow(dfsGraphAsset);
                //执行子蓝图中实际的端口调用链检查
                SubPortData subPortData = (SubPortData)portData;
                BaseNodeData asSubNodeData = m_SubGraphAsset.FindNodeData(subPortData.GetAsSubNodeID());
                if (asSubNodeData != null)
                {
                    BasePortData asSubPortData = asSubNodeData.FindPortData(subPortData.GetAsSubPortID());
                    if (asSubPortData != null)
                    {
                        DFSGraphAsset subDFSGraphAsset = DFSGraphAsset.Allocate(m_SubGraphAsset, ((SubNodeSliceData)nodeSliceData).SubGraphSliceData);
                        subDFSGraphAsset.SetParent(dfsGraphAsset);
                        //检查子蓝图中实际端口调用链，这一步会出现检查调用链需要返回到父蓝图，父蓝图又会
                        asSubNodeData.DFSExecutionFlow(subDFSGraphAsset, asSubPortData);
                        DFSGraphAsset.Release(subDFSGraphAsset);
                    }
                }
            }
        }
#if UNITY_EDITOR
        [SerializeField] private int m_SourcePortID;

        private static ExposePortsNodeData[] InitializeExposePortsData(GraphAsset subGraphAsset)
        {
            ExposePortsNodeData[] exposePortsNodesData = ArrayPool<ExposePortsNodeData>.Shared.Rent(2);
            if (subGraphAsset != null)
            {
                int count = 0;
                IReadOnlyList<BaseNodeData> nodesData = subGraphAsset.GetNodesData();
                for (int i = 0; i < nodesData.Count; i++)
                {
                    if (nodesData[i] is ExposePortsNodeData exposePortsNodeData)
                    {
                        //确保初始化非序列化数据
                        exposePortsNodeData.InitializePortData();
                        exposePortsNodesData[count++] = exposePortsNodeData;
                        if (count > 1)
                        {
                            break;
                        }
                    }
                }
            }
            return exposePortsNodesData;
        }

        private static ExposePortData FindExposePortData(ExposePortsNodeData[] exposePortsNodesData, int asSubNodeID, int asSubPortID)
        {
            for (int i = 0; i < exposePortsNodesData.Length; i++)
            {
                ExposePortsNodeData exposePortsNodeData = exposePortsNodesData[i];
                if (exposePortsNodeData != null)
                {
                    IReadOnlyList<ExposePortData> exposePortsData = exposePortsNodeData.GetExposePortsData();
                    for (int j = 0; j < exposePortsData.Count; j++)
                    {
                        ExposePortData exposePortData = exposePortsData[j];
                        PortConnectionData exposePortAddress = exposePortData.GetToExposePortAddress();
                        if (exposePortAddress.NodeID == asSubNodeID && exposePortAddress.PortID == asSubPortID)
                        {
                            return exposePortData;
                        }
                    }
                }
            }
            return null;
        }

        public void SetSubGraphAsset(GraphAsset subGraphAsset)
        {
            if (m_SubGraphAsset != subGraphAsset)
            {
                for (int i = 0; i < m_SubPortsData.Count; i++)
                {
                    m_SubPortsData[i].DisconnectAll();
                }
                m_SubPortsData.Clear();
                //确保蓝图引用初始化
                subGraphAsset.InitializeReference();
                ExposePortsNodeData[] exposePortsNodesData = InitializeExposePortsData(subGraphAsset);
                for (int i = 0; i < exposePortsNodesData.Length; i++)
                {
                    ExposePortsNodeData exposePortsNodeData = exposePortsNodesData[i];
                    if (exposePortsNodeData != null)
                    {
                        IReadOnlyList<ExposePortData> exposePortsData = exposePortsNodeData.GetExposePortsData();
                        for (int j = 0; j < exposePortsData.Count; j++)
                        {
                            ExposePortData exposePortData = exposePortsData[j];
                            BasePortData asSubPortData = exposePortData.GetToExposePortData();
                            if (asSubPortData != null)
                            {
                                SubPortData subPortData = new(asSubPortData.CreateSubPortData(), exposePortData.GetToExposePortAddress().NodeID, exposePortData.GetToExposePortAddress().PortID);
                                subPortData.SetPortID(++m_SourcePortID);
                                subPortData.SetNodeData(this);
                                m_SubPortsData.Add(subPortData);
                            }
                        }
                    }
                }
                ArrayPool<ExposePortsNodeData>.Shared.Return(exposePortsNodesData);
            }
        }

        public void OnExposePortDataConnectionChanged(ExposePortData exposePortData, int asSubNodeID, int asSubPortID, bool isConnect)
        {
            if (isConnect)
            {
                SubPortData subPortData = new(exposePortData.GetToExposePortData().CreateSubPortData(), asSubNodeID, asSubPortID);
                subPortData.SetPortID(++m_SourcePortID);
                subPortData.SetNodeData(this);
                m_SubPortsData.Add(subPortData);
            }
            else
            {
                int index = FindSubPortDataIndexBySubPortAddress(asSubNodeID, asSubPortID);
                if (index != -1)
                {
                    m_SubPortsData[index].DisconnectAll();
                    m_SubPortsData.RemoveAt(index);
                }
            }
        }
        
        public void InitializeSubPortsData()
        {
            ExposePortsNodeData[] exposePortsData = InitializeExposePortsData(m_SubGraphAsset);
            for (int i = 0; i < m_SubPortsData.Count; i++)
            {
                SubPortData subPortData = m_SubPortsData[i];
                subPortData.SetFieldName($"{nameof(m_SubPortsData)}.Array.data[{i}]");
                ExposePortData exposePortData = FindExposePortData(exposePortsData, subPortData.GetAsSubNodeID(), subPortData.GetAsSubPortID());
                if (exposePortData != null)
                {
                    subPortData.SetPortName(exposePortData.ExposePortDisplayName);
                    subPortData.RevertNonSerializedData(exposePortData.GetToExposePortData());
                }
                else
                {
                    Debug.LogError(
                        $"{nameof(SubNodeData)} node id:{m_NodeID} port id:{subPortData.GetPortID()} has saved a missing port data,address:node id:{subPortData.GetAsSubNodeID()} port id:{subPortData.GetAsSubPortID()}");
                }
            }
            ArrayPool<ExposePortsNodeData>.Shared.Return(exposePortsData);
        }

        public override void InitializeSerializedData()
        {
            m_SubPortsData = new List<SubPortData>();
        }

        protected override void OnInitializePortData()
        {
            InitializeSubPortsData();
        }
#endif
    }
}