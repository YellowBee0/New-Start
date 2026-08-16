using System;
using UnityEngine;
using YBFramework.Bridge.Data;
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
            int portCount = GetIndexPortDataCount();
            for (int i = 0; i < portCount; i++)
            {
                BasePortData portData = IndexPortData(i);
                if (portData.GetPortID() == portID)
                {
                    return portData;
                }
            }
            return null;
        }

        public abstract int GetIndexPortDataCount();

        public abstract BasePortData IndexPortData(int index);

        public abstract BaseNode CreateRuntimeInstance();
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