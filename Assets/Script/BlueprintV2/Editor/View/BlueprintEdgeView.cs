using System;
using UnityEditor.Experimental.GraphView;

namespace YBFramework.BlueprintV2.Editor
{
    /// <summary>
    /// 连接的纯视图对象，只记录稳定 ID 与两端地址，不持有连接数据实例。
    /// </summary>
    public class BlueprintEdgeView : Edge
    {
        private readonly Type m_ModelType;

        public BlueprintEdgeView(BlueprintConnectionData connection, BlueprintPortReference owner)
        {
            if (connection == null)
            {
                throw new ArgumentNullException(nameof(connection));
            }

            ConnectionId = connection.Id;
            Owner = owner;
            Target = connection.Target;
            m_ModelType = connection.GetType();
        }

        public BlueprintConnectionId ConnectionId { get; }

        public BlueprintPortReference Owner { get; }

        public BlueprintPortReference Target { get; }

        public bool Matches(BlueprintConnectionData connection, BlueprintPortReference owner)
        {
            return connection != null &&
                   connection.Id == ConnectionId &&
                   connection.GetType() == m_ModelType &&
                   owner == Owner &&
                   connection.Target == Target;
        }

        public virtual void Refresh(BlueprintConnectionData connection)
        {
        }
    }
}
