using System;
using System.Collections.Generic;
using UnityEngine;

namespace YBFramework.BlueprintV2
{
    public enum BlueprintPortDirection
    {
        Input,
        Output
    }

    public enum BlueprintPortCapacity
    {
        Single,
        Multiple
    }

    /// <summary>
    /// 可扩展的端口模型。完整连接保存在 owner 端，另一端仅保存 incoming 反向索引。
    /// </summary>
    [Serializable]
    public class BlueprintPortData
    {
        [SerializeField] private BlueprintPortId m_Id;
        [SerializeField] private string m_DisplayName;
        [SerializeField] private BlueprintPortDirection m_Direction;
        [SerializeField] private BlueprintPortCapacity m_Capacity;
        [SerializeField] private Color m_Color = Color.white;
        // 这里只表示“端口自身显示数据”是否变化，不因连接集合变化而递增。
        // 因此单纯 Connect/Disconnect 在 Undo 时只会操作 Edge，不会刷新 Port View。
        [SerializeField, HideInInspector] private int m_ViewRevision;
        // SerializeReference 允许不同端口创建不同的连接数据派生类型。
        [SerializeReference] private List<BlueprintConnectionData> m_OwnedConnections = new();
        // 非权威的轻量反向索引，必须与 owner 端的完整连接同时增删。
        [SerializeField] private List<BlueprintIncomingConnection> m_IncomingConnections = new();

        // 运行时回指不参与序列化；Undo 或反序列化后由 BlueprintAsset 统一重建。
        [NonSerialized] private BlueprintNodeData m_Node;

        public BlueprintPortData()
        {
        }

        public BlueprintPortData(string displayName, BlueprintPortDirection direction, BlueprintPortCapacity capacity)
        {
            m_Id = BlueprintPortId.Create();
            m_DisplayName = displayName;
            m_Direction = direction;
            m_Capacity = capacity;
        }

        public BlueprintPortId Id => m_Id;

        public string DisplayName => m_DisplayName;

        public BlueprintPortDirection Direction => m_Direction;

        public BlueprintPortCapacity Capacity => m_Capacity;

        public Color Color => m_Color;

        public BlueprintNodeData Node => m_Node;

        public int ViewRevision => m_ViewRevision;

        public BlueprintPortReference Reference => new BlueprintPortReference(m_Node == null ? default : m_Node.Id, m_Id);

        public IReadOnlyList<BlueprintConnectionData> OwnedConnections
        {
            get
            {
                EnsureCollectionsInternal();
                return m_OwnedConnections;
            }
        }

        public IReadOnlyList<BlueprintIncomingConnection> IncomingConnections
        {
            get
            {
                EnsureCollectionsInternal();
                return m_IncomingConnections;
            }
        }

        public int TotalConnectionCount
        {
            get
            {
                EnsureCollectionsInternal();
                return m_OwnedConnections.Count + m_IncomingConnections.Count;
            }
        }

        /// <summary>
        /// 判断当前端口能否成为连接数据的唯一所有者。
        /// 两端策略的结果必须恰好一个为 true，否则 EditService 会拒绝连接。
        /// </summary>
        public virtual bool CanOwnConnection(BlueprintPortData target, out string reason)
        {
            if (target == null)
            {
                reason = "Target port is null.";
                return false;
            }
            if (Reference == target.Reference)
            {
                reason = "A port cannot connect to itself.";
                return false;
            }
            if (m_Direction != BlueprintPortDirection.Output || target.m_Direction != BlueprintPortDirection.Input)
            {
                reason = "The default port policy only allows Output to Input connections.";
                return false;
            }
            reason = null;
            return true;
        }

        /// <summary>
        /// 创建当前端口专用的连接数据。Action、Value 等端口可返回各自的派生类型。
        /// </summary>
        protected virtual BlueprintConnectionData CreateConnectionData(BlueprintPortData target)
        {
            return new BlueprintConnectionData();
        }

        protected void Configure(string displayName, BlueprintPortDirection direction, BlueprintPortCapacity capacity, Color color)
        {
            m_DisplayName = displayName;
            m_Direction = direction;
            m_Capacity = capacity;
            m_Color = color;
        }

        internal void EnsureIdentityInternal()
        {
            if (!m_Id.IsValid)
            {
                m_Id = BlueprintPortId.Create();
            }
            EnsureCollectionsInternal();
        }

        internal void AttachInternal(BlueprintNodeData node)
        {
            m_Node = node;
            EnsureIdentityInternal();
        }

        internal BlueprintConnectionData AddOwnedConnectionInternal(BlueprintPortData target)
        {
            EnsureCollectionsInternal();
            BlueprintConnectionData connection = CreateConnectionData(target);
            if (connection == null)
            {
                throw new InvalidOperationException($"{GetType().Name}.{nameof(CreateConnectionData)} returned null.");
            }
            connection.InitializeInternal(target.Reference);
            m_OwnedConnections.Add(connection);
            return connection;
        }

        internal bool RemoveOwnedConnectionInternal(BlueprintConnectionId connectionId)
        {
            EnsureCollectionsInternal();
            for (int i = 0; i < m_OwnedConnections.Count; i++)
            {
                BlueprintConnectionData connection = m_OwnedConnections[i];
                if (connection != null && connection.Id == connectionId)
                {
                    m_OwnedConnections.RemoveAt(i);
                    return true;
                }
            }
            return false;
        }

        internal void AddIncomingConnectionInternal(BlueprintConnectionId connectionId, BlueprintPortReference owner)
        {
            EnsureCollectionsInternal();
            if (!HasIncomingConnectionInternal(connectionId, owner))
            {
                m_IncomingConnections.Add(new BlueprintIncomingConnection(connectionId, owner));
            }
        }

        internal bool RemoveIncomingConnectionInternal(BlueprintConnectionId connectionId, BlueprintPortReference owner)
        {
            EnsureCollectionsInternal();
            for (int i = 0; i < m_IncomingConnections.Count; i++)
            {
                BlueprintIncomingConnection incoming = m_IncomingConnections[i];
                if (incoming.ConnectionId == connectionId && incoming.Owner == owner)
                {
                    m_IncomingConnections.RemoveAt(i);
                    return true;
                }
            }
            return false;
        }

        internal bool HasIncomingConnectionInternal(BlueprintConnectionId connectionId, BlueprintPortReference owner)
        {
            EnsureCollectionsInternal();
            for (int i = 0; i < m_IncomingConnections.Count; i++)
            {
                BlueprintIncomingConnection incoming = m_IncomingConnections[i];
                if (incoming.ConnectionId == connectionId && incoming.Owner == owner)
                {
                    return true;
                }
            }
            return false;
        }

        internal void SetDisplayNameInternal(string displayName)
        {
            m_DisplayName = displayName;
            IncrementViewRevisionInternal();
        }

        internal void IncrementViewRevisionInternal()
        {
            unchecked
            {
                m_ViewRevision++;
            }
        }

        private void EnsureCollectionsInternal()
        {
            m_OwnedConnections ??= new List<BlueprintConnectionData>();
            m_IncomingConnections ??= new List<BlueprintIncomingConnection>();
        }
    }
}
