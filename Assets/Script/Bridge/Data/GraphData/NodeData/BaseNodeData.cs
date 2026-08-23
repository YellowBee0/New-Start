using System;
using System.Collections.Generic;
using UnityEngine;
using YBFramework.GameLogic.Graph;

namespace YBFramework.Bridge.NewData
{
    [Serializable]
    public abstract class BaseNodeData
    {
        [SerializeField] protected int m_NodeID;

        public int GetNodeID()
        {
            return m_NodeID;
        }

        public BasePortData FindPortData(int portID)
        {
            int portCount = GetPortsDataCount();
            for (int i = 0; i < portCount; i++)
            {
                BasePortData portData = PortDataOfIndex(i);
                if (portData.GetPortID() == portID)
                {
                    return portData;
                }
            }
            return null;
        }

        public abstract int GetPortsDataCount();

        public abstract BasePortData PortDataOfIndex(int index);

        public abstract BaseNode CreateRuntimeInstance(NodeDataOnCallChain nodeDataOnCallChain);
        
        public virtual void GetCallChain(GraphAsset graphAsset, in Dictionary<BaseNodeData, HashSet<NodeDataOnCallChain>> nodesDataOnChain)
        {
        }

        protected virtual void LinkOtherPortData(GraphAsset graphAsset, in Dictionary<BaseNodeData, HashSet<BasePortData>> dataOnChain, BasePortData portData)
        {
        }

        public virtual void FilterRuntimePortData(GraphAsset graphAsset, int portID)
        {
            int portDataCount = GetPortsDataCount();
            for (int i = 0; i < portDataCount; i++)
            {
                BasePortData portData = PortDataOfIndex(i);
                if (portData.GetPortID() == portID)
                {
                    int portConnectionDataCount = portData.GetPortConnectionsDataCount();
                    for (int j = 0; j < portConnectionDataCount; j++)
                    {
                        PortConnectionData portConnectionData = portData.PortConnectionDataOfIndex(j);
                        if (portConnectionData != null)
                        {
                            BaseNodeData nodeData = graphAsset.FindNodeData(portConnectionData.NodeID);
                            nodeData?.FilterRuntimePortData(graphAsset, portConnectionData.PortID);
                        }
                    }
                    //添加PortData到创建集合
                }
            }
        }
#if UNITY_EDITOR
        private GraphAsset m_GraphAsset;

        private bool m_IsInitializePortData;

        public GraphAsset GetGraphAsset()
        {
            return m_GraphAsset;
        }

        public void InitializePortData()
        {
            if (!m_IsInitializePortData)
            {
                OnInitializePortData();
                m_IsInitializePortData = true;
            }
        }

        protected abstract void OnInitializePortData();

        public abstract void InitializeSerializedData();

        public abstract void InitializePortDataView();
#endif
    }
}