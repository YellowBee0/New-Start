#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using YBFramework.GameLogic.Graph;

namespace YBFramework.Bridge.Data
{
    [Serializable]
    public sealed class ProxyTargetNodeData : BaseNodeData
    {
        [SerializeField] private List<ProxyTargetPortData> m_ProxyTargetPortData;

        public override BaseNode CreateRuntimeInstance()
        {
            return null;
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