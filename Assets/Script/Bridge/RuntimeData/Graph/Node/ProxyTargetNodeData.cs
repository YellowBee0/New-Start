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
#if UNITY_EDITOR
    [NodeMenu("代理端口集合", GraphType.Everything)]
    [NodeExistCountLimit(2)]
#endif
    public sealed class ProxyTargetNodeData : BaseNodeData
    {
        [SerializeField] private List<ProxyTargetPortData> ProxyTargetPortData;

        [SerializeField] public bool IsProxyInput;

        public override BaseNode CreateRuntimeInstance()
        {
            throw new Exception("Proxy target node can not create runtime node");
        }

        public override bool Iterator(int index, out BasePortData current)
        {
            if (ProxyTargetPortData != null && index < ProxyTargetPortData.Count)
            {
                current = ProxyTargetPortData[index];
                return true;
            }
            current = null;
            return false;
        }

        public override void Initialize()
        {
            base.Initialize();
            for (int i = 0; i < ProxyTargetPortData.Count; i++)
            {
                ProxyTargetPortData proxyTargetPortData = ProxyTargetPortData[i];
                proxyTargetPortData.SetPortName(proxyTargetPortData.ProxyName);
                proxyTargetPortData.SetDirection(IsProxyInput ? Direction.Input : Direction.Output);
                proxyTargetPortData.SetCapacity(Port.Capacity.Single);
                proxyTargetPortData.SetPortColor(Color.green);
            }
        }
    }
}
#endif