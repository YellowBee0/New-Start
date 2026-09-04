#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using YBFramework.Bridge.Editor;
using YBFramework.GameLogic.Graph;

namespace YBFramework.Bridge.Data
{
    [Serializable]
    [NodeMenu("暴露端口给子图节点", GraphType.Everything)]
    [NodeExistCountLimit(2)]
    public sealed class ExposeNodeData : BaseNodeData
    {
        private const string PORT_NAME = "暴露端口{0}";

        [SerializeField] private List<ExposePortData> m_ExposePortsData = new();

        [SerializeField] private int m_SourcePortID;

        [SerializeField] private bool m_IsInput;

        public IReadOnlyList<ExposePortData> GetExposePortsData()
        {
            return m_ExposePortsData;
        }

        public bool GetIsInput()
        {
            return m_IsInput;
        }

        public void AddExposePortData(ExposePortData exposePortData)
        {
            exposePortData.SetNodeData(this);
            exposePortData.SetPortID(++m_SourcePortID);
            m_ExposePortsData.Add(exposePortData);
        }

        public void RemoveExposePortData(ExposePortData exposePortData)
        {
            m_ExposePortsData.Remove(exposePortData);
        }

        public void ChangeDirection(bool isInput)
        {
            if (m_IsInput != isInput)
            {
                for (int i = 0; i < m_ExposePortsData.Count; i++)
                {
                    m_ExposePortsData[i].DisconnectAll();
                }
                m_ExposePortsData.Clear();
                m_IsInput = isInput;
            }
        }

        public void InitializeExposePortDataView(ExposePortData exposePortData)
        {
            for (int i = 0; i < m_ExposePortsData.Count; i++)
            {
                if (m_ExposePortsData[i] == exposePortData)
                {
                    Initialize(exposePortData, i);
                    break;
                }
            }
        }

        public void RefreshExposePortDataName()
        {
            for (int i = 0; i < m_ExposePortsData.Count; i++)
            {
                m_ExposePortsData[i].SetPortName(string.Format(PORT_NAME, i));
            }
        }

        private void Initialize(ExposePortData exposePortData, int index)
        {
            exposePortData.SetPortName(string.Format(PORT_NAME, index));
            exposePortData.SetDirection(m_IsInput ? Direction.Output : Direction.Input);
            exposePortData.SetPortColor(Color.green);
        }

        public override int GetPortsDataCount()
        {
            return m_ExposePortsData.Count;
        }

        public override BasePortData PortDataOfIndex(int index)
        {
            return m_ExposePortsData[index];
        }

        public override BaseNode CreateRuntimeInstance(NodeSliceData nodeSliceData)
        {
            throw new InvalidOperationException($"{nameof(ExposeNodeData)} will never call {nameof(CreateRuntimeInstance)} method");
        }

        public override void DFSExecutionFlow(DFSGraphAsset dfsGraphAsset, BasePortData portData)
        {
            throw new InvalidOperationException($"{nameof(ExposeNodeData)} will never call {nameof(DFSExecutionFlow)} method");
        }

        public override void InitializeSerializedData()
        {
            m_ExposePortsData = new List<ExposePortData>();
        }

        protected override void OnInitializePortData()
        {
            for (int i = 0; i < m_ExposePortsData.Count; i++)
            {
                ExposePortData exposePortData = m_ExposePortsData[i];
                Initialize(exposePortData, i);
                PortConnectionData exposePortAddress = exposePortData.GetToExposePortAddress();
                if (exposePortAddress.IsValid())
                {
                    BaseNodeData toExposeNodeData = m_GraphAsset.FindNodeData(exposePortAddress.NodeID);
                    if (toExposeNodeData != null)
                    {
                        BasePortData toExposePortData = toExposeNodeData.FindPortData(exposePortAddress.PortID);
                        if (toExposePortData != null)
                        {
                            //确保子蓝图中的子节点初始化
                            toExposeNodeData.InitializePortData();
                        }
                        exposePortData.SetToExposePortData(toExposePortData);
                    }
                    if (exposePortData.GetToExposePortData() == null)
                    {
                        Debug.LogError(
                            $"Expose port id:{exposePortData.GetPortID()} in node id:{GetNodeID()} saved port id:{exposePortAddress.PortID} in node id:{exposePortAddress.NodeID} isn't exist");
                    }
                }
            }
        }
    }
}
#endif