using System;
using UnityEngine;

namespace YBFramework.BlueprintV2
{
    // Unity Undo 可能重新反序列化 SerializeReference 对象，因此不能用托管对象引用标识图元素。
    // 这里生成的字符串 ID 会随资产一起序列化，是数据层与视图层对齐的稳定身份。
    internal static class BlueprintIdUtility
    {
        public static string Create()
        {
            return Guid.NewGuid().ToString("N");
        }
    }

    /// <summary>
    /// 节点的持久化身份。Undo/Redo 后节点实例可能改变，但该 ID 保持不变。
    /// </summary>
    [Serializable]
    public struct BlueprintNodeId : IEquatable<BlueprintNodeId>
    {
        [SerializeField] private string m_Value;

        internal static BlueprintNodeId Create()
        {
            return new BlueprintNodeId { m_Value = BlueprintIdUtility.Create() };
        }

        public bool IsValid => !string.IsNullOrEmpty(m_Value);

        public string Value => m_Value;

        public bool Equals(BlueprintNodeId other)
        {
            return string.Equals(m_Value, other.m_Value, StringComparison.Ordinal);
        }

        public override bool Equals(object obj)
        {
            return obj is BlueprintNodeId other && Equals(other);
        }

        public override int GetHashCode()
        {
            return m_Value == null ? 0 : StringComparer.Ordinal.GetHashCode(m_Value);
        }

        public override string ToString()
        {
            return m_Value ?? string.Empty;
        }

        public static bool operator ==(BlueprintNodeId left, BlueprintNodeId right) => left.Equals(right);

        public static bool operator !=(BlueprintNodeId left, BlueprintNodeId right) => !left.Equals(right);
    }

    /// <summary>
    /// 端口在所属节点内的持久化身份，与 NodeId 组合后才能唯一定位一个端口。
    /// </summary>
    [Serializable]
    public struct BlueprintPortId : IEquatable<BlueprintPortId>
    {
        [SerializeField] private string m_Value;

        internal static BlueprintPortId Create()
        {
            return new BlueprintPortId { m_Value = BlueprintIdUtility.Create() };
        }

        public bool IsValid => !string.IsNullOrEmpty(m_Value);

        public string Value => m_Value;

        public bool Equals(BlueprintPortId other)
        {
            return string.Equals(m_Value, other.m_Value, StringComparison.Ordinal);
        }

        public override bool Equals(object obj)
        {
            return obj is BlueprintPortId other && Equals(other);
        }

        public override int GetHashCode()
        {
            return m_Value == null ? 0 : StringComparer.Ordinal.GetHashCode(m_Value);
        }

        public override string ToString()
        {
            return m_Value ?? string.Empty;
        }

        public static bool operator ==(BlueprintPortId left, BlueprintPortId right) => left.Equals(right);

        public static bool operator !=(BlueprintPortId left, BlueprintPortId right) => !left.Equals(right);
    }

    /// <summary>
    /// 连接的持久化身份。视图层使用它局部新增、移除或刷新 Edge。
    /// </summary>
    [Serializable]
    public struct BlueprintConnectionId : IEquatable<BlueprintConnectionId>
    {
        [SerializeField] private string m_Value;

        internal static BlueprintConnectionId Create()
        {
            return new BlueprintConnectionId { m_Value = BlueprintIdUtility.Create() };
        }

        public bool IsValid => !string.IsNullOrEmpty(m_Value);

        public string Value => m_Value;

        public bool Equals(BlueprintConnectionId other)
        {
            return string.Equals(m_Value, other.m_Value, StringComparison.Ordinal);
        }

        public override bool Equals(object obj)
        {
            return obj is BlueprintConnectionId other && Equals(other);
        }

        public override int GetHashCode()
        {
            return m_Value == null ? 0 : StringComparer.Ordinal.GetHashCode(m_Value);
        }

        public override string ToString()
        {
            return m_Value ?? string.Empty;
        }

        public static bool operator ==(BlueprintConnectionId left, BlueprintConnectionId right) => left.Equals(right);

        public static bool operator !=(BlueprintConnectionId left, BlueprintConnectionId right) => !left.Equals(right);
    }
}
