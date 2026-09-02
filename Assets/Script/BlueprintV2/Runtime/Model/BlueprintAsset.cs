using System.Collections.Generic;
using UnityEngine;

namespace YBFramework.BlueprintV2
{
    /// <summary>
    /// 蓝图的聚合根，也是 Unity Undo 实际记录的对象。
    /// 节点、端口及连接都位于同一个序列化对象图中，因此一次 Undo 能原子恢复完整关系。
    /// </summary>
    [CreateAssetMenu(menuName = "Blueprint V2/Blueprint Asset", fileName = "NewBlueprint")]
    public class BlueprintAsset : ScriptableObject
    {
        [SerializeReference] private List<BlueprintNodeData> m_Nodes = new();

        // 查询索引仅用于运行时加速，Unity Undo 恢复后必须重新构建。
        [System.NonSerialized] private Dictionary<BlueprintNodeId, BlueprintNodeData> m_NodeIndex;

        public IReadOnlyList<BlueprintNodeData> Nodes
        {
            get
            {
                m_Nodes ??= new List<BlueprintNodeData>();
                return m_Nodes;
            }
        }

        public bool TryGetNode(BlueprintNodeId nodeId, out BlueprintNodeData node)
        {
            EnsureIndexInternal();
            return m_NodeIndex.TryGetValue(nodeId, out node);
        }

        public bool TryResolvePort(BlueprintPortReference reference, out BlueprintPortData port)
        {
            if (TryGetNode(reference.NodeId, out BlueprintNodeData node))
            {
                return node.TryGetPort(reference.PortId, out port);
            }
            port = null;
            return false;
        }

        protected virtual void OnEnable()
        {
            RebuildNonSerializedStateInternal();
        }

        internal void RebuildNonSerializedStateInternal()
        {
            m_Nodes ??= new List<BlueprintNodeData>();
            m_NodeIndex ??= new Dictionary<BlueprintNodeId, BlueprintNodeData>();
            m_NodeIndex.Clear();
            for (int i = 0; i < m_Nodes.Count; i++)
            {
                BlueprintNodeData node = m_Nodes[i];
                if (node == null)
                {
                    continue;
                }
                node.AttachInternal(this);
                // 损坏资产中的重复 ID 由 Validator 报告，这里保留第一项以保证编辑器仍能打开。
                if (!m_NodeIndex.ContainsKey(node.Id))
                {
                    m_NodeIndex.Add(node.Id, node);
                }
            }
        }

        internal bool AddNodeInternal(BlueprintNodeData node)
        {
            if (node == null)
            {
                return false;
            }
            EnsureIndexInternal();
            node.AttachInternal(this);
            if (m_NodeIndex.ContainsKey(node.Id))
            {
                return false;
            }
            m_Nodes.Add(node);
            m_NodeIndex.Add(node.Id, node);
            return true;
        }

        internal bool RemoveNodeInternal(BlueprintNodeId nodeId)
        {
            for (int i = 0; i < m_Nodes.Count; i++)
            {
                BlueprintNodeData node = m_Nodes[i];
                if (node != null && node.Id == nodeId)
                {
                    m_Nodes.RemoveAt(i);
                    m_NodeIndex?.Remove(nodeId);
                    return true;
                }
            }
            return false;
        }

        internal bool TryFindOwnedConnectionInternal(
            BlueprintConnectionId connectionId,
            out BlueprintPortData owner,
            out BlueprintConnectionData connection)
        {
            for (int i = 0; i < m_Nodes.Count; i++)
            {
                BlueprintNodeData node = m_Nodes[i];
                if (node == null)
                {
                    continue;
                }
                IReadOnlyList<BlueprintPortData> ports = node.Ports;
                for (int j = 0; j < ports.Count; j++)
                {
                    BlueprintPortData port = ports[j];
                    if (port == null)
                    {
                        continue;
                    }
                    IReadOnlyList<BlueprintConnectionData> connections = port.OwnedConnections;
                    for (int k = 0; k < connections.Count; k++)
                    {
                        BlueprintConnectionData candidate = connections[k];
                        if (candidate != null && candidate.Id == connectionId)
                        {
                            owner = port;
                            connection = candidate;
                            return true;
                        }
                    }
                }
            }
            owner = null;
            connection = null;
            return false;
        }

        internal bool AreConnectedInternal(BlueprintPortReference first, BlueprintPortReference second)
        {
            return HasOwnedConnectionToInternal(first, second) || HasOwnedConnectionToInternal(second, first);
        }

        private bool HasOwnedConnectionToInternal(BlueprintPortReference ownerReference, BlueprintPortReference targetReference)
        {
            if (!TryResolvePort(ownerReference, out BlueprintPortData owner))
            {
                return false;
            }
            IReadOnlyList<BlueprintConnectionData> connections = owner.OwnedConnections;
            for (int i = 0; i < connections.Count; i++)
            {
                BlueprintConnectionData connection = connections[i];
                if (connection != null && connection.Target == targetReference)
                {
                    return true;
                }
            }
            return false;
        }

        private void EnsureIndexInternal()
        {
            if (m_NodeIndex == null)
            {
                RebuildNonSerializedStateInternal();
            }
        }
    }
}
