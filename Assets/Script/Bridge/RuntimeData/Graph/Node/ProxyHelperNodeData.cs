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
    [NodeMenu("蓝图代理辅助", GraphType.Everything)]
    [NodeExistCountLimit(2)]
    public sealed class ProxyHelperNodeData : BaseNodeData
    {
        [SerializeField] private List<ProxyHelperPortData> m_ProxyHelperPortsData;

        [SerializeField] private int m_ProxyHelperPortDataIDRecord;

        public bool IsInputPortsProxyHelper;

        /// <summary>
        /// 当作为代理的蓝图里面代理端口发生新增或者删除时调用这个委托。
        /// 新增或者删除都是在蓝图视图中发生。
        /// 参数1为节点id，参数2为端口id，参数3为是否为新增true/false代表新增/删除
        /// </summary>
        private Action<int, int, bool> m_OnProxyDataChanged;

        public IReadOnlyList<ProxyHelperPortData> GetProxyHelperPortsData()
        {
            return m_ProxyHelperPortsData;
        }

        public int AllocateProxyHelperPortDataID()
        {
            return ++m_ProxyHelperPortDataIDRecord;
        }

        public void InitializeProxyHelperPortData(ProxyHelperPortData proxyHelperPortData, int index)
        {
            proxyHelperPortData.SetNodeData(this);
            proxyHelperPortData.SetFiledName($"{nameof(m_ProxyHelperPortsData)}.Array.data[{index}]");
            proxyHelperPortData.SetPortName($"代理目标端口{index}");
            proxyHelperPortData.SetDirection(IsInputPortsProxyHelper ? Direction.Input : Direction.Output);
            proxyHelperPortData.SetPortColor(Color.green);
            proxyHelperPortData.SetCapacity(Port.Capacity.Single);
            //端口刚创建出来或者没有连接其他端口时连线会为0，0
            if (proxyHelperPortData.TargetPortConnectionData.NodeID != 0 && proxyHelperPortData.TargetPortConnectionData.PortID != 0)
            {
                BaseNodeData nodeData = m_GraphAsset.GetNodeData(proxyHelperPortData.TargetPortConnectionData.NodeID);
                BasePortData portData = nodeData.GetPortData(proxyHelperPortData.TargetPortConnectionData.PortID);
                proxyHelperPortData.SetTargetPortData(portData);
            }
        }

        public void AddProxyHelperPortData(ProxyHelperPortData portData)
        {
            m_ProxyHelperPortsData.Add(portData);
        }

        public void RemoveProxyHelperPortData(ProxyHelperPortData portData)
        {
            m_ProxyHelperPortsData.Remove(portData);
        }

        public override BaseNode CreateRuntimeInstance()
        {
            Debug.Log("Editor only node:proxy helper node is tried to create a runtime node");
            return null;
        }

        public override bool Iterator(int index, out BasePortData current)
        {
            if (index < m_ProxyHelperPortsData.Count)
            {
                current = m_ProxyHelperPortsData[index];
                return true;
            }
            current = null;
            return false;
        }

        public override void InitializeSerializedData()
        {
            m_ProxyHelperPortsData = new List<ProxyHelperPortData>();
        }

        public override void Initialize()
        {
            for (int i = 0; i < m_ProxyHelperPortsData.Count; i++)
            {
                InitializeProxyHelperPortData(m_ProxyHelperPortsData[i], i);
            }
        }
    }
}
#endif