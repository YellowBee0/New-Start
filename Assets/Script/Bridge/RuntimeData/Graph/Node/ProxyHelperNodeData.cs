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
        public const string PORT_HELPER_DATA_PATH = nameof(ProxyHelperPortsData) + ".Array.data[{0}]";

        public const string PORT_HELPER_NAME = "代理目标端口{0}";

        public const Port.Capacity DEFAULT_PORT_CAPACITY = Port.Capacity.Single;

        public static readonly Color DefaultColor = Color.green;

        /// <summary>
        /// 当作为代理的蓝图里面代理端口发生新增或者删除时调用这个委托。
        /// 新增或者删除都是在蓝图视图中发生。
        /// 参数1为节点id，参数2为端口id，参数3为是否为新增true/false代表新增/删除
        /// </summary>
        public Action<int, int, bool> OnProxyDataChanged;

        public List<ProxyHelperPortData> ProxyHelperPortsData;

        public int PortID;

        public bool IsInputPortsProxyHelper;

        public void CreateProxyHelperPortData()
        {
            ProxyHelperPortData proxyHelperPortData = CreatePortData<ProxyHelperPortData>(++PortID);
            InitializeProxyHelperPortData(proxyHelperPortData,ProxyHelperPortsData.Count);
            ProxyHelperPortsData.Add(proxyHelperPortData);
        }
        
        public ProxyHelperPortData InitializeSerializedProxyHelperPortData()
        {
            ProxyHelperPortData proxyHelperPortData = CreatePortData<ProxyHelperPortData>(++PortID);
            InitializeProxyHelperPortData(proxyHelperPortData,ProxyHelperPortsData.Count);
            ProxyHelperPortsData.Add(proxyHelperPortData);
            return proxyHelperPortData;
        }

        private void InitializeProxyHelperPortData(ProxyHelperPortData proxyHelperPortData, int index)
        {
            proxyHelperPortData.SetNodeData(this);
            proxyHelperPortData.SetFiledName(string.Format(PORT_HELPER_DATA_PATH, index));
            proxyHelperPortData.SetPortName(string.Format(PORT_HELPER_NAME, index));
            proxyHelperPortData.SetDirection(IsInputPortsProxyHelper ? Direction.Input : Direction.Output);
            proxyHelperPortData.SetPortColor(DefaultColor);
            proxyHelperPortData.SetCapacity(DEFAULT_PORT_CAPACITY);
            //正常情况下nodeData和portData都不会为null
            if (proxyHelperPortData.TargetPortConnectionData.NodeID == 0 || proxyHelperPortData.TargetPortConnectionData.PortID == 0)
            {
                Debug.LogWarning($"Port id:{proxyHelperPortData.PortID} did not connect any other port");
                return;
            }
            BaseNodeData nodeData = m_GraphAsset.GetNodeData(proxyHelperPortData.TargetPortConnectionData.NodeID);
            BasePortData portData = nodeData.GetPortData(proxyHelperPortData.TargetPortConnectionData.PortID);
            proxyHelperPortData.SetTargetPortData(portData);
        }
        
        public override BaseNode CreateRuntimeInstance()
        {
            Debug.Log("Editor only node:proxy helper node is tried to create a runtime node");
            return null;
        }

        public override bool Iterator(int index, out BasePortData current)
        {
            if (index < ProxyHelperPortsData.Count)
            {
                current = ProxyHelperPortsData[index];
                return true;
            }
            current = null;
            return false;
        }

        public override void CreateData()
        {
            ProxyHelperPortsData = new List<ProxyHelperPortData>();
        }

        public override void Initialize()
        {
            base.Initialize();
            Direction direction = IsInputPortsProxyHelper ? Direction.Input : Direction.Output;
            for (int i = 0; i < ProxyHelperPortsData.Count; i++)
            {
                ProxyHelperPortData proxyHelperPortData = ProxyHelperPortsData[i];
                proxyHelperPortData.SetFiledName(string.Format(PORT_HELPER_DATA_PATH, i));
                proxyHelperPortData.SetPortName(string.Format(PORT_HELPER_NAME, i));
                proxyHelperPortData.SetDirection(direction);
                proxyHelperPortData.SetPortColor(DefaultColor);
                proxyHelperPortData.SetCapacity(DEFAULT_PORT_CAPACITY);
                //正常情况下nodeData和portData都不会为null
                if (proxyHelperPortData.TargetPortConnectionData.NodeID == 0 || proxyHelperPortData.TargetPortConnectionData.PortID == 0)
                {
                    Debug.LogWarning($"Port id:{proxyHelperPortData.PortID} did not connect any other port");
                    continue;
                }
                BaseNodeData nodeData = m_GraphAsset.GetNodeData(proxyHelperPortData.TargetPortConnectionData.NodeID);
                BasePortData portData = nodeData.GetPortData(proxyHelperPortData.TargetPortConnectionData.PortID);
                proxyHelperPortData.SetTargetPortData(portData);
            }
        }
    }
}
#endif