using System;
using System.Collections.Generic;
using UnityEngine;
using YBFramework.GameLogic.Graph;
#if UNITY_EDITOR
using System.Buffers;
#endif

namespace YBFramework.Bridge.NewData
{
    [Serializable]
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
                if (subPortData.GetSubNodeID() == subNodeID && subPortData.GetSubPortID() == subPortID)
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
            DFSGraphAsset.Free(subDFSGraphAsset);
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
                BaseNodeData nodeDataInSubGraphAsset = m_SubGraphAsset.FindNodeData(subPortData.GetSubNodeID());
                if (nodeDataInSubGraphAsset != null)
                {
                    BasePortData portDataInSubGraphAsset = nodeDataInSubGraphAsset.FindPortData(subPortData.GetSubPortID());
                    if (portDataInSubGraphAsset != null)
                    {
                        DFSGraphAsset subDFSGraphAsset = DFSGraphAsset.Allocate(m_SubGraphAsset, ((SubNodeSliceData)nodeSliceData).SubGraphSliceData);
                        subDFSGraphAsset.SetParent(dfsGraphAsset);
                        //检查子蓝图中实际端口调用链，这一步会出现检查调用链需要返回到父蓝图，父蓝图又会
                        nodeDataInSubGraphAsset.DFSExecutionFlow(subDFSGraphAsset, portDataInSubGraphAsset);
                        DFSGraphAsset.Free(subDFSGraphAsset);
                    }
                }
            }
        }
#if UNITY_EDITOR
        private static SubNodeDataBridge[] InitializeSubNodeDataBridges(GraphAsset subGraphAsset)
        {
            SubNodeDataBridge[] subNodeDataBridges = ArrayPool<SubNodeDataBridge>.Shared.Rent(2);
            if (subGraphAsset != null)
            {
                int count = 0;
                IReadOnlyList<BaseNodeData> nodesData = subGraphAsset.GetNodesData();
                for (int i = 0; i < nodesData.Count; i++)
                {
                    if (nodesData[i] is SubNodeDataBridge subNodeDataBridge)
                    {
                        //确保初始化非序列化数据
                        subNodeDataBridge.InitializePortData();
                        subNodeDataBridges[count++] = subNodeDataBridge;
                        if (count > 1)
                        {
                            break;
                        }
                    }
                }
            }
            return subNodeDataBridges;
        }

        private static SubPortDataBridge FindSubPortDataBridge(SubNodeDataBridge[] subNodeDataBridges, int subNodeID, int subPortID)
        {
            for (int i = 0; i < subNodeDataBridges.Length; i++)
            {
                SubNodeDataBridge subNodeDataBridge = subNodeDataBridges[i];
                if (subNodeDataBridge != null)
                {
                    IReadOnlyList<SubPortDataBridge> subPortDataBridges = subNodeDataBridge.GetSubPortDataBridges();
                    for (int j = 0; j < subPortDataBridges.Count; j++)
                    {
                        SubPortDataBridge subPortDataBridge = subPortDataBridges[j];
                        PortConnectionData subPortDataAddress = subPortDataBridge.GetSubPortAddress();
                        if (subPortDataAddress.NodeID == subNodeID && subPortDataAddress.PortID == subPortID)
                        {
                            return subPortDataBridge;
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
                SubNodeDataBridge[] subNodeDataBridges = InitializeSubNodeDataBridges(subGraphAsset);
                for (int i = 0; i < subNodeDataBridges.Length; i++)
                {
                    SubNodeDataBridge subNodeDataBridge = subNodeDataBridges[i];
                    if (subNodeDataBridge != null)
                    {
                        IReadOnlyList<SubPortDataBridge> subPortDataBridges = subNodeDataBridge.GetSubPortDataBridges();
                        for (int j = 0; j < subPortDataBridges.Count; j++)
                        {
                            SubPortDataBridge subPortDataBridge = subPortDataBridges[j];
                            SubPortData subPortData = new(subPortDataBridge.GetSubPortData().CreateSubPortData(), subPortDataBridge.GetSubPortAddress().NodeID,
                                subPortDataBridge.GetSubPortAddress().PortID);
                            m_SubPortsData.Add(subPortData);
                        }
                    }
                }
                ArrayPool<SubNodeDataBridge>.Shared.Return(subNodeDataBridges);
            }
        }

        public void OnSubPortDataBridgeConnectionChanged(SubPortDataBridge subPortDataBridge, int subNodeID, int subPortID, bool isConnect)
        {
            if (isConnect)
            {
                SubPortData subPortData = new(subPortDataBridge.GetSubPortData().CreateSubPortData(), subNodeID, subPortID);
                m_SubPortsData.Add(subPortData);
            }
            else
            {
                int index = FindSubPortDataIndexBySubPortAddress(subNodeID, subPortID);
                if (index != -1)
                {
                    m_SubPortsData[index].DisconnectAll();
                    m_SubPortsData.RemoveAt(index);
                }
            }
        }

        //TODO:在Presenter中设置了SubGraphAsset后调用
        public void InitializeSubPortsData()
        {
            SubNodeDataBridge[] subNodeDataBridges = InitializeSubNodeDataBridges(m_SubGraphAsset);
            for (int i = 0; i < m_SubPortsData.Count; i++)
            {
                SubPortData subPortData = m_SubPortsData[i];
                subPortData.SetFieldName($"{nameof(m_SubPortsData)}.Array.data[{i}]");
                SubPortDataBridge subPortDataBridge = FindSubPortDataBridge(subNodeDataBridges, subPortData.GetSubNodeID(), subPortData.GetSubPortID());
                if (subPortDataBridge != null)
                {
                    subPortData.SetPortName(string.IsNullOrEmpty(subPortDataBridge.SubPortDisplayName) ? subPortDataBridge.GetSubPortData().GetPortName() : subPortDataBridge.SubPortDisplayName);
                    subPortData.RevertNonSerializedData(subPortDataBridge.GetSubPortData());
                }
                else
                {
                    Debug.LogError(
                        $"{nameof(SubNodeData)} node id:{m_NodeID} port id:{subPortData.GetPortID()} has saved a missing port data,address:node id:{subPortData.GetSubNodeID()} port id:{subPortData.GetSubPortID()}");
                }
            }
            ArrayPool<SubNodeDataBridge>.Shared.Return(subNodeDataBridges);
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