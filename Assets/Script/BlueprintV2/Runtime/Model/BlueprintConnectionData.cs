using System;
using UnityEngine;

namespace YBFramework.BlueprintV2
{
    /// <summary>
    /// 一条连接的完整数据。该对象只序列化在连接所有者端口上，派生类可加入具体业务字段。
    /// </summary>
    [Serializable]
    public class BlueprintConnectionData
    {
        [SerializeField] private BlueprintConnectionId m_Id;
        [SerializeField] private BlueprintPortReference m_Target;
        // 业务字段通过 EditService 修改时递增。Undo 会恢复旧 revision，投影层据此只刷新该 Edge。
        [SerializeField, HideInInspector] private int m_ViewRevision;

        public BlueprintConnectionId Id => m_Id;

        public BlueprintPortReference Target => m_Target;

        public int ViewRevision => m_ViewRevision;

        internal void IncrementViewRevisionInternal()
        {
            unchecked
            {
                m_ViewRevision++;
            }
        }

        internal void InitializeInternal(BlueprintPortReference target)
        {
            if (!m_Id.IsValid)
            {
                m_Id = BlueprintConnectionId.Create();
            }
            m_Target = target;
        }
    }

    /// <summary>
    /// 目标端口上的轻量反向索引，不重复保存具体连接数据。
    /// 它让端口能够快速枚举所有入向连接，同时仍由 owner 保存权威连接数据。
    /// </summary>
    [Serializable]
    public struct BlueprintIncomingConnection
    {
        [SerializeField] private BlueprintConnectionId m_ConnectionId;
        [SerializeField] private BlueprintPortReference m_Owner;

        public BlueprintIncomingConnection(BlueprintConnectionId connectionId, BlueprintPortReference owner)
        {
            m_ConnectionId = connectionId;
            m_Owner = owner;
        }

        public BlueprintConnectionId ConnectionId => m_ConnectionId;

        public BlueprintPortReference Owner => m_Owner;
    }
}
