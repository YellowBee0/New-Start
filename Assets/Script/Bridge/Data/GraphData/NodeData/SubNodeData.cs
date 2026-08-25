using System;
using System.Collections.Generic;
using UnityEngine;
using YBFramework.GameLogic.Graph;

namespace YBFramework.Bridge.NewData
{
    [Serializable]
    public sealed class SubNodeData : BaseNodeData
    {
        [SerializeField] private GraphAsset m_SubGraphAsset;

        [SerializeField] private List<SubPortData> m_SubPortsData;

        public SubPortData FindSubPortDataBySubPortID(int subPortID)
        {
            for (int i = 0; i < m_SubPortsData.Count; i++)
            {
                SubPortData subPortData = m_SubPortsData[i];
                if (subPortData.GetSubPortID() == subPortID)
                {
                    return subPortData;
                }
            }
            return null;
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
        protected override void OnInitializePortData()
        {
            throw new NotImplementedException();
        }

        public override void InitializeSerializedData()
        {
            m_SubPortsData = new List<SubPortData>();
        }

        public override void InitializePortDataView()
        {
            throw new NotImplementedException();
        }
#endif
    }
}