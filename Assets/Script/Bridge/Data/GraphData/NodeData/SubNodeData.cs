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
            ProxyNode  proxyNode = new ProxyNode();
            proxyNode.InitializeFromProxyNodeData(this, (SubNodeDataOnCallChain)nodeDataOnCallChain);
            return proxyNode;
        }

        protected override void LinkOtherPortData(GraphAsset graphAsset, in Dictionary<BaseNodeData, HashSet<BasePortData>> dataOnChain, BasePortData portData)
        {
            if (!dataOnChain.TryGetValue(this, out HashSet<BasePortData> portsDataOnChain))
            {
                portsDataOnChain = new HashSet<BasePortData>();
                dataOnChain.Add(this, portsDataOnChain);
            }
            if (portsDataOnChain.Add(portData))
            {
                //dataOnChain是一个新的数据，而不是现在这个
                portData.LinkOtherPortData(m_SubGraphAsset, dataOnChain);
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