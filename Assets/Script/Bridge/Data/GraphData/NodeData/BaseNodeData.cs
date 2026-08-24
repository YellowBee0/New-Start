using System;
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

        public abstract BaseNode CreateRuntimeInstance(NodeSliceData nodeSliceData);

        public virtual void CheckExecutionSliceEntry(DFSGraphAsset dfsGraphAsset)
        {
        }

        public abstract void DFSExecutionFlow(DFSGraphAsset dfsGraphAsset, BasePortData portData);
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