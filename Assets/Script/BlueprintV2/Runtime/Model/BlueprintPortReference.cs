using System;
using UnityEngine;

namespace YBFramework.BlueprintV2
{
    /// <summary>
    /// 可序列化的端口地址。不要保存 BlueprintPortData 实例引用来跨 Undo 定位端口。
    /// </summary>
    [Serializable]
    public struct BlueprintPortReference : IEquatable<BlueprintPortReference>
    {
        [SerializeField] private BlueprintNodeId m_NodeId;
        [SerializeField] private BlueprintPortId m_PortId;

        public BlueprintPortReference(BlueprintNodeId nodeId, BlueprintPortId portId)
        {
            m_NodeId = nodeId;
            m_PortId = portId;
        }

        public BlueprintNodeId NodeId => m_NodeId;

        public BlueprintPortId PortId => m_PortId;

        public bool IsValid => m_NodeId.IsValid && m_PortId.IsValid;

        public bool Equals(BlueprintPortReference other)
        {
            return m_NodeId.Equals(other.m_NodeId) && m_PortId.Equals(other.m_PortId);
        }

        public override bool Equals(object obj)
        {
            return obj is BlueprintPortReference other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (m_NodeId.GetHashCode() * 397) ^ m_PortId.GetHashCode();
            }
        }

        public override string ToString()
        {
            return $"{m_NodeId}/{m_PortId}";
        }

        public static bool operator ==(BlueprintPortReference left, BlueprintPortReference right) => left.Equals(right);

        public static bool operator !=(BlueprintPortReference left, BlueprintPortReference right) => !left.Equals(right);
    }
}
