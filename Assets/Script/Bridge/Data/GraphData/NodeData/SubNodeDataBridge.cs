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
    public sealed class SubNodeDataBridge : BaseNodeData
    {
        private const string PORT_NAME = "暴露端口{0}";

        [SerializeField] private List<SubPortDataBridge> m_SubPortDataBridges = new();

        public bool IsInputPortValid;

        public IReadOnlyList<SubPortDataBridge> GetSubPortDataBridges()
        {
            return m_SubPortDataBridges;
        }

        public void AddSubPortDataBridge(SubPortDataBridge subPortDataBridge)
        {
            m_SubPortDataBridges.Add(subPortDataBridge);
        }

        public void RemoveSubPortDataBridge(SubPortDataBridge subPortDataBridge)
        {
            m_SubPortDataBridges.Remove(subPortDataBridge);
        }

        public void InitializeSubPortDataBridgePortView(SubPortDataBridge subPortDataBridge)
        {
            for (int i = 0; i < m_SubPortDataBridges.Count; i++)
            {
                if (m_SubPortDataBridges[i] == subPortDataBridge)
                {
                    Initialize(subPortDataBridge, i);
                    break;
                }
            }
        }

        public void RefreshSubPortDataBridgePortName()
        {
            for (int i = 0; i < m_SubPortDataBridges.Count; i++)
            {
                m_SubPortDataBridges[i].SetPortName(string.Format(PORT_NAME, i));
            }
        }

        private void Initialize(SubPortDataBridge subPortDataBridge, int index)
        {
            subPortDataBridge.SetFieldName($"{nameof(m_SubPortDataBridges)}.Array.data[{index}]");
            subPortDataBridge.SetPortName(string.Format(PORT_NAME, index));
            subPortDataBridge.SetDirection(IsInputPortValid ? Direction.Output : Direction.Input);
            subPortDataBridge.SetPortColor(Color.green);
        }

        public override int GetPortsDataCount()
        {
            return m_SubPortDataBridges.Count;
        }

        public override BasePortData PortDataOfIndex(int index)
        {
            return m_SubPortDataBridges[index];
        }

        public override BaseNode CreateRuntimeInstance(NodeSliceData nodeSliceData)
        {
            throw new InvalidOperationException($"{nameof(SubNodeDataBridge)} will never call {nameof(CreateRuntimeInstance)} method");
        }

        public override void DFSExecutionFlow(DFSGraphAsset dfsGraphAsset, BasePortData portData)
        {
            throw new InvalidOperationException($"{nameof(SubNodeDataBridge)} will never call {nameof(DFSExecutionFlow)} method");
        }

        public override void InitializeSerializedData()
        {
            m_SubPortDataBridges = new List<SubPortDataBridge>();
        }

        protected override void OnInitializePortData()
        {
            for (int i = 0; i < m_SubPortDataBridges.Count; i++)
            {
                SubPortDataBridge subPortDataBridge = m_SubPortDataBridges[i];
                Initialize(subPortDataBridge, i);
                PortConnectionData subPortAddress = subPortDataBridge.GetSubPortAddress();
                if (subPortAddress.NodeID > 0 && subPortAddress.PortID > 0)
                {
                    BaseNodeData subNodeData = m_GraphAsset.FindNodeData(subPortAddress.NodeID);
                    if (subNodeData != null)
                    {
                        BasePortData subPortData = subNodeData.FindPortData(subPortAddress.PortID);
                        if (subPortData != null)
                        {
                            //确保子蓝图中的子节点初始化
                            subNodeData.InitializeSerializedData();
                        }
                        subPortDataBridge.SetSubPortData(subPortData);
                    }
                    if (subPortDataBridge.GetSubPortData() == null)
                    {
                        Debug.LogError($"Port id:{subPortDataBridge.GetPortID()} in node id:{GetNodeID()} connected port id:{subPortAddress.PortID} in node id:{subPortAddress.NodeID} isn't exist");
                    }
                }
            }
        }
    }
}
#endif