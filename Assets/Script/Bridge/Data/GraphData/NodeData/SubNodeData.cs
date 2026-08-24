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

        public SubPortData GetSubPortDataBySubPortID(int subPortID)
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

        public override BaseNode CreateRuntimeInstance(NodeDataOnCallChain nodeDataOnCallChain)
        {
            ProxyNode proxyNode = new ProxyNode();
            proxyNode.InitializeFromProxyNodeData(this, (SubNodeDataOnCallChain)nodeDataOnCallChain);
            return proxyNode;
        }

        public override void LinkOtherPort(CheckValidStack checkValidStack, in Dictionary<BaseNodeData, NodeDataOnCallChain> validNodesData, BasePortData portData)
        {
            if (!validNodesData.TryGetValue(this, out NodeDataOnCallChain portsDataOnChain))
            {
                portsDataOnChain = new SubNodeDataOnCallChain();
                validNodesData.Add(this, portsDataOnChain);
            }
            SubPortData subPortData = (SubPortData)portData;
            if (portsDataOnChain.AddPortDataOnCallChain(subPortData))
            {
                //执行子端口在当前蓝图调用链的检查
                subPortData.LinkOtherPort(checkValidStack, portsDataOnChain);
                //执行子蓝图中实际的端口调用链检查
                BaseNodeData subNodeData = m_SubGraphAsset.FindNodeData(subPortData.GetSubNodeID());
                if (subNodeData != null)
                {
                    portData = subNodeData.FindPortData(subPortData.GetSubPortID());
                    if (portData != null)
                    {
                        //检查使用子蓝图的现场
                        CheckValidStack subCheckValidStack = CheckValidStack.Allocate(m_SubGraphAsset);
                        subCheckValidStack.SetParentStack(checkValidStack);
                        //检查子蓝图中实际端口调用链，这一步会出现检查调用链需要返回到父蓝图，父蓝图又会
                        subNodeData.LinkOtherPort(subCheckValidStack, ((SubNodeDataOnCallChain)portsDataOnChain).GetSubNodesDataOnCallChain(), portData);
                        CheckValidStack.Free(subCheckValidStack);
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