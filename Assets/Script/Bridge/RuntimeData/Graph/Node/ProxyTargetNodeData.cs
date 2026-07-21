#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UIElements;
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
        [SerializeField] private List<ProxyTargetPortData> m_ProxyTargetPortData;

        [FormerlySerializedAs("m_IsProxyInput")] [SerializeField]
        public bool IsProxyInput;

        private Toggle m_IsProxyInputToggle;

        public override BaseNode CreateRuntimeInstance()
        {
            throw new Exception("Proxy target node can not create runtime node");
        }

        public override bool Iterator(int index, out BasePortData current)
        {
            if (m_ProxyTargetPortData != null && index < m_ProxyTargetPortData.Count)
            {
                current = m_ProxyTargetPortData[index];
                return true;
            }
            current = null;
            return false;
        }

        public override void Initialize()
        {
            base.Initialize();
            for (int i = 0; i < m_ProxyTargetPortData.Count; i++)
            {
                ProxyTargetPortData proxyTargetPortData = m_ProxyTargetPortData[i];
                proxyTargetPortData.SetPortViewArgs(proxyTargetPortData.ProxyName, Direction.Input, Port.Capacity.Multi, Color.green);
            }
        }
    }
}
#endif