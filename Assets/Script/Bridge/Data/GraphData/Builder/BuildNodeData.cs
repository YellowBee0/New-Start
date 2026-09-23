using System;
using System.Buffers;
using System.Collections.Generic;
using YBFramework.GameLogic.Graph;

namespace YBFramework.Bridge.Data
{
    public struct BuildNodeData : IEquatable<BuildNodeData>
    {
        public readonly BaseNodeData NodeData;

        private readonly (BasePortData, BasePort)[] m_BuildPortsData;

        private int m_Count;

        public BuildNodeData(BaseNodeData nodeData, int size)
        {
            NodeData = nodeData;
            m_BuildPortsData = ArrayPool<(BasePortData, BasePort)>.Shared.Rent(size);
            m_Count = 0;
        }

        public int GetCount()
        {
            return m_Count;
        }

        public IReadOnlyList<(BasePortData, BasePort)> GetBuildPortsData()
        {
            return m_BuildPortsData;
        }

        public BasePort FindBuildPort(int portID)
        {
            for (int i = 0; i < m_BuildPortsData.Length; i++)
            {
                (BasePortData, BasePort) nodeDataMapping = m_BuildPortsData[i];
                if (nodeDataMapping != default && nodeDataMapping.Item1.GetPortID() == portID)
                {
                    return nodeDataMapping.Item2;
                }
            }
            return null;
        }

        public void AddBuildPortData(BasePortData portData, BasePort port)
        {
            m_BuildPortsData[m_Count++] = new ValueTuple<BasePortData, BasePort>(portData, port);
        }

        public void Dispose()
        {
            ArrayPool<(BasePortData, BasePort)>.Shared.Return(m_BuildPortsData, true);
        }

        public bool Equals(BuildNodeData other)
        {
            return Equals(m_BuildPortsData, other.m_BuildPortsData);
        }

        public override bool Equals(object obj)
        {
            return obj is BuildNodeData other && Equals(other);
        }

        public override int GetHashCode()
        {
            return (m_BuildPortsData != null ? m_BuildPortsData.GetHashCode() : 0);
        }

        public static bool operator ==(BuildNodeData left, BuildNodeData right)
        {
            return left.m_BuildPortsData == right.m_BuildPortsData;
        }

        public static bool operator !=(BuildNodeData left, BuildNodeData right)
        {
            return !(left == right);
        }
    }
}