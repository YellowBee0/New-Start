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
    [NodeMenu("代理端口集合", GraphType.Everything)]
    [NodeExistCountLimit(2)]
    public sealed class ProxyTargetNodeData : BaseNodeData
    {
        public const string DEFAULT_FILED_NAME = nameof(ProxyTargetPortsData) + ".Array.data[{0}]";

        public const string DEFAULT_PORT_NAME = "代理目标端口{0}";

        public const Port.Capacity DEFAULT_PORT_CAPACITY = Port.Capacity.Single;

        public static readonly Color DefaultColor = Color.green;

        public List<ProxyTargetPortData> ProxyTargetPortsData;

        public bool IsProxyInput;

        public override BaseNode CreateRuntimeInstance()
        {
            throw new Exception("Proxy target node can not create runtime node");
        }

        public override bool Iterator(int index, out BasePortData current)
        {
            if (ProxyTargetPortsData != null && index < ProxyTargetPortsData.Count)
            {
                current = ProxyTargetPortsData[index];
                return true;
            }
            current = null;
            return false;
        }

        public override void CreateData()
        {
            ProxyTargetPortsData = new List<ProxyTargetPortData>();
        }

        public override void Initialize()
        {
            base.Initialize();
            Direction direction = IsProxyInput ? Direction.Input : Direction.Output;
            for (int i = 0; i < ProxyTargetPortsData.Count; i++)
            {
                ProxyTargetPortData proxyTargetPortData = ProxyTargetPortsData[i];
                proxyTargetPortData.SetFiledName(string.Format(DEFAULT_FILED_NAME, i));
                proxyTargetPortData.SetPortName(string.Format(DEFAULT_PORT_NAME, i));
                proxyTargetPortData.SetDirection(direction);
                proxyTargetPortData.SetPortColor(DefaultColor);
                proxyTargetPortData.SetCapacity(DEFAULT_PORT_CAPACITY);
            }
        }
    }
}
#endif