using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace YBFramework.BlueprintV2.Editor
{
    /// <summary>
    /// BlueprintAsset 的唯一写入口。负责 Undo 事务、连接两端的一致性、变更集和外部同步通知。
    /// View/Controller 不得绕过该服务直接修改模型。
    /// </summary>
    public sealed class BlueprintEditService
    {
        public static event Action<BlueprintAsset, BlueprintChangeSet> GraphChanged;

        private readonly BlueprintAsset m_Asset;

        public BlueprintEditService(BlueprintAsset asset)
        {
            m_Asset = asset != null ? asset : throw new ArgumentNullException(nameof(asset));
            m_Asset.RebuildNonSerializedStateInternal();
            BlueprintUndoCoordinator.Track(m_Asset);
        }

        public BlueprintAsset Asset => m_Asset;

        public bool CanConnect(BlueprintPortReference first, BlueprintPortReference second, out string reason)
        {
            if (!m_Asset.TryResolvePort(first, out BlueprintPortData firstPort) ||
                !m_Asset.TryResolvePort(second, out BlueprintPortData secondPort))
            {
                reason = "One or both ports do not exist.";
                return false;
            }
            if (m_Asset.AreConnectedInternal(first, second))
            {
                reason = "The ports are already connected.";
                return false;
            }
            bool firstCanOwn = firstPort.CanOwnConnection(secondPort, out string firstReason);
            bool secondCanOwn = secondPort.CanOwnConnection(firstPort, out string secondReason);
            // 连接数据必须只有一个权威 owner。两端都拥有或两端都不拥有都会造成语义不确定。
            if (firstCanOwn == secondCanOwn)
            {
                reason = firstCanOwn
                    ? "Both ports claim ownership of the same connection. The policy must select exactly one owner."
                    : $"Neither port can own the connection. First: {firstReason} Second: {secondReason}";
                return false;
            }
            reason = null;
            return true;
        }

        public BlueprintEditResult AddNode(BlueprintNodeData node)
        {
            if (node == null)
            {
                return BlueprintEditResult.Failure("Node is null.");
            }
            if (node.Asset != null && node.Asset != m_Asset)
            {
                return BlueprintEditResult.Failure("The node already belongs to another blueprint asset.");
            }
            node.EnsureIdentityInternal();
            if (m_Asset.TryGetNode(node.Id, out _))
            {
                return BlueprintEditResult.Failure($"Node id {node.Id} already exists in this graph.");
            }
            return Execute("Add Blueprint Node", () =>
            {
                BlueprintChangeSet changes = new BlueprintChangeSet();
                if (!m_Asset.AddNodeInternal(node))
                {
                    return changes;
                }
                changes.MarkNodeAdded(node.Id);
                IReadOnlyList<BlueprintPortData> ports = node.Ports;
                for (int i = 0; i < ports.Count; i++)
                {
                    if (ports[i] != null)
                    {
                        changes.MarkPortAdded(ports[i].Reference);
                    }
                }
                return changes;
            });
        }

        public BlueprintEditResult RemoveNode(BlueprintNodeId nodeId)
        {
            return RemoveElements(new[] { nodeId }, Array.Empty<BlueprintConnectionId>());
        }

        public BlueprintEditResult RemoveElements(
            IReadOnlyCollection<BlueprintNodeId> nodeIds,
            IReadOnlyCollection<BlueprintConnectionId> connectionIds)
        {
            HashSet<BlueprintNodeId> existingNodes = new HashSet<BlueprintNodeId>();
            foreach (BlueprintNodeId nodeId in nodeIds)
            {
                if (m_Asset.TryGetNode(nodeId, out _))
                {
                    existingNodes.Add(nodeId);
                }
            }
            HashSet<BlueprintConnectionId> requestedConnections = new HashSet<BlueprintConnectionId>(connectionIds);
            if (existingNodes.Count == 0 && requestedConnections.Count == 0)
            {
                return BlueprintEditResult.Failure("No existing graph elements were selected for removal.");
            }

            return Execute("Remove Blueprint Elements", () =>
            {
                BlueprintChangeSet changes = new BlueprintChangeSet();
                // 删除节点前先收集其所有入向/出向连接，使双端索引在同一个 Undo 事务内一起移除。
                foreach (BlueprintNodeId nodeId in existingNodes)
                {
                    if (!m_Asset.TryGetNode(nodeId, out BlueprintNodeData node))
                    {
                        continue;
                    }
                    IReadOnlyList<BlueprintPortData> ports = node.Ports;
                    for (int i = 0; i < ports.Count; i++)
                    {
                        BlueprintPortData port = ports[i];
                        if (port == null)
                        {
                            continue;
                        }
                        CollectConnectionIds(port, requestedConnections);
                    }
                }
                foreach (BlueprintConnectionId connectionId in requestedConnections)
                {
                    DisconnectInternal(connectionId, changes);
                }
                foreach (BlueprintNodeId nodeId in existingNodes)
                {
                    if (!m_Asset.TryGetNode(nodeId, out BlueprintNodeData node))
                    {
                        continue;
                    }
                    IReadOnlyList<BlueprintPortData> ports = node.Ports;
                    for (int i = 0; i < ports.Count; i++)
                    {
                        if (ports[i] != null)
                        {
                            changes.MarkPortRemoved(ports[i].Reference);
                        }
                    }
                    if (m_Asset.RemoveNodeInternal(nodeId))
                    {
                        changes.MarkNodeRemoved(nodeId);
                    }
                }
                return changes;
            });
        }

        public BlueprintEditResult AddPort(BlueprintNodeId nodeId, BlueprintPortData port)
        {
            if (port == null)
            {
                return BlueprintEditResult.Failure("Port is null.");
            }
            if (!m_Asset.TryGetNode(nodeId, out BlueprintNodeData node))
            {
                return BlueprintEditResult.Failure($"Node {nodeId} does not exist.");
            }
            if (port.Node != null && port.Node != node)
            {
                return BlueprintEditResult.Failure("The port already belongs to another node.");
            }
            port.EnsureIdentityInternal();
            if (node.TryGetPort(port.Id, out _))
            {
                return BlueprintEditResult.Failure($"Port id {port.Id} already exists on node {nodeId}.");
            }
            return Execute("Add Blueprint Port", () =>
            {
                BlueprintChangeSet changes = new BlueprintChangeSet();
                if (node.AddPortInternal(port))
                {
                    changes.MarkPortAdded(port.Reference);
                }
                return changes;
            });
        }

        public BlueprintEditResult RemovePort(BlueprintPortReference portReference)
        {
            if (!m_Asset.TryResolvePort(portReference, out BlueprintPortData port) || port.Node == null)
            {
                return BlueprintEditResult.Failure($"Port {portReference} does not exist.");
            }
            return Execute("Remove Blueprint Port", () =>
            {
                BlueprintChangeSet changes = new BlueprintChangeSet();
                HashSet<BlueprintConnectionId> connectionIds = new HashSet<BlueprintConnectionId>();
                CollectConnectionIds(port, connectionIds);
                foreach (BlueprintConnectionId connectionId in connectionIds)
                {
                    DisconnectInternal(connectionId, changes);
                }
                if (port.Node.RemovePortInternal(portReference.PortId))
                {
                    changes.MarkPortRemoved(portReference);
                }
                return changes;
            });
        }

        public BlueprintEditResult Connect(BlueprintPortReference first, BlueprintPortReference second)
        {
            if (!CanConnect(first, second, out string reason))
            {
                return BlueprintEditResult.Failure(reason);
            }
            m_Asset.TryResolvePort(first, out BlueprintPortData firstPort);
            m_Asset.TryResolvePort(second, out BlueprintPortData secondPort);
            bool firstOwns = firstPort.CanOwnConnection(secondPort, out _);
            BlueprintPortData owner = firstOwns ? firstPort : secondPort;
            BlueprintPortData target = firstOwns ? secondPort : firstPort;

            return Execute("Connect Blueprint Ports", () =>
            {
                BlueprintChangeSet changes = new BlueprintChangeSet();
                HashSet<BlueprintConnectionId> replacements = new HashSet<BlueprintConnectionId>();
                // Single 端口的旧连接与新连接合并在一个 Undo 组，撤销一次即可完整恢复旧状态。
                if (owner.Capacity == BlueprintPortCapacity.Single)
                {
                    CollectConnectionIds(owner, replacements);
                }
                if (target.Capacity == BlueprintPortCapacity.Single)
                {
                    CollectConnectionIds(target, replacements);
                }
                foreach (BlueprintConnectionId connectionId in replacements)
                {
                    DisconnectInternal(connectionId, changes);
                }

                // owner 保存完整多态连接，target 只保存反向索引；两步都发生在同一资产事务内。
                BlueprintConnectionData connection = owner.AddOwnedConnectionInternal(target);
                target.AddIncomingConnectionInternal(connection.Id, owner.Reference);
                changes.MarkConnectionAdded(connection.Id);
                return changes;
            });
        }

        public BlueprintEditResult Disconnect(BlueprintConnectionId connectionId)
        {
            if (!m_Asset.TryFindOwnedConnectionInternal(connectionId, out _, out _))
            {
                return BlueprintEditResult.Failure($"Connection {connectionId} does not exist.");
            }
            return Execute("Disconnect Blueprint Ports", () =>
            {
                BlueprintChangeSet changes = new BlueprintChangeSet();
                DisconnectInternal(connectionId, changes);
                return changes;
            });
        }

        public BlueprintEditResult SetNodeTitle(BlueprintNodeId nodeId, string title)
        {
            if (!m_Asset.TryGetNode(nodeId, out BlueprintNodeData node))
            {
                return BlueprintEditResult.Failure($"Node {nodeId} does not exist.");
            }
            if (node.Title == title)
            {
                return BlueprintEditResult.Success(new BlueprintChangeSet());
            }
            return Execute("Rename Blueprint Node", () =>
            {
                node.SetTitleInternal(title);
                BlueprintChangeSet changes = new BlueprintChangeSet();
                changes.MarkNodeChanged(nodeId);
                return changes;
            });
        }

        public BlueprintEditResult SetNodePositions(IReadOnlyDictionary<BlueprintNodeId, Vector2> positions)
        {
            bool hasChanges = false;
            foreach (KeyValuePair<BlueprintNodeId, Vector2> pair in positions)
            {
                if (m_Asset.TryGetNode(pair.Key, out BlueprintNodeData node) && node.Position != pair.Value)
                {
                    hasChanges = true;
                    break;
                }
            }
            if (!hasChanges)
            {
                return BlueprintEditResult.Success(new BlueprintChangeSet());
            }
            return Execute("Move Blueprint Nodes", () =>
            {
                BlueprintChangeSet changes = new BlueprintChangeSet();
                foreach (KeyValuePair<BlueprintNodeId, Vector2> pair in positions)
                {
                    if (m_Asset.TryGetNode(pair.Key, out BlueprintNodeData node) && node.Position != pair.Value)
                    {
                        node.SetPositionInternal(pair.Value);
                        changes.MarkNodeChanged(pair.Key);
                    }
                }
                return changes;
            });
        }

        /// <summary>
        /// 修改派生节点字段的通用入口。mutation 只改数据，Undo 与 ViewRevision 由服务统一处理。
        /// </summary>
        public BlueprintEditResult ModifyNode<TNode>(
            BlueprintNodeId nodeId,
            string undoName,
            Action<TNode> mutation)
            where TNode : BlueprintNodeData
        {
            if (!m_Asset.TryGetNode(nodeId, out BlueprintNodeData node) || node is not TNode typedNode)
            {
                return BlueprintEditResult.Failure($"Node {nodeId} is not a {typeof(TNode).Name}.");
            }
            if (mutation == null)
            {
                return BlueprintEditResult.Failure("Mutation is null.");
            }

            return Execute(string.IsNullOrEmpty(undoName) ? "Modify Blueprint Node" : undoName, () =>
            {
                mutation(typedNode);
                typedNode.IncrementViewRevisionInternal();
                BlueprintChangeSet changes = new BlueprintChangeSet();
                changes.MarkNodeChanged(nodeId);
                return changes;
            });
        }

        /// <summary>
        /// 修改派生端口字段的通用入口。方向/容量改变时还会重新对齐该端口已有的 Edge。
        /// </summary>
        public BlueprintEditResult ModifyPort<TPort>(
            BlueprintPortReference portReference,
            string undoName,
            Action<TPort> mutation)
            where TPort : BlueprintPortData
        {
            if (!m_Asset.TryResolvePort(portReference, out BlueprintPortData port) || port is not TPort typedPort)
            {
                return BlueprintEditResult.Failure($"Port {portReference} is not a {typeof(TPort).Name}.");
            }
            if (mutation == null)
            {
                return BlueprintEditResult.Failure("Mutation is null.");
            }

            return Execute(string.IsNullOrEmpty(undoName) ? "Modify Blueprint Port" : undoName, () =>
            {
                BlueprintPortDirection oldDirection = typedPort.Direction;
                BlueprintPortCapacity oldCapacity = typedPort.Capacity;
                mutation(typedPort);
                typedPort.IncrementViewRevisionInternal();
                BlueprintChangeSet changes = new BlueprintChangeSet();
                changes.MarkPortChanged(portReference);
                if (oldDirection != typedPort.Direction || oldCapacity != typedPort.Capacity)
                {
                    HashSet<BlueprintConnectionId> affectedConnections = new HashSet<BlueprintConnectionId>();
                    CollectConnectionIds(typedPort, affectedConnections);
                    foreach (BlueprintConnectionId connectionId in affectedConnections)
                    {
                        changes.MarkConnectionChanged(connectionId);
                    }
                }
                return changes;
            });
        }

        /// <summary>
        /// 修改具体连接数据字段的通用入口，只标记对应 Edge 需要刷新。
        /// </summary>
        public BlueprintEditResult ModifyConnection<TConnection>(
            BlueprintConnectionId connectionId,
            string undoName,
            Action<TConnection> mutation)
            where TConnection : BlueprintConnectionData
        {
            if (!m_Asset.TryFindOwnedConnectionInternal(
                    connectionId,
                    out _,
                    out BlueprintConnectionData connection) ||
                connection is not TConnection typedConnection)
            {
                return BlueprintEditResult.Failure($"Connection {connectionId} is not a {typeof(TConnection).Name}.");
            }
            if (mutation == null)
            {
                return BlueprintEditResult.Failure("Mutation is null.");
            }

            return Execute(string.IsNullOrEmpty(undoName) ? "Modify Blueprint Connection" : undoName, () =>
            {
                mutation(typedConnection);
                typedConnection.IncrementViewRevisionInternal();
                BlueprintChangeSet changes = new BlueprintChangeSet();
                changes.MarkConnectionChanged(connectionId);
                return changes;
            });
        }

        private BlueprintEditResult Execute(string undoName, Func<BlueprintChangeSet> mutation)
        {
            // 必须在任何序列化字段修改之前记录聚合根，确保节点、端口及连接能原子恢复。
            Undo.IncrementCurrentGroup();
            int undoGroup = Undo.GetCurrentGroup();
            Undo.SetCurrentGroupName(undoName);
            Undo.RegisterCompleteObjectUndo(m_Asset, undoName);
            BlueprintChangeSet changes = mutation();
            // mutation 可能增删 SerializeReference 对象，立即重建所有非序列化索引，禁止沿用旧实例。
            m_Asset.RebuildNonSerializedStateInternal();
            EditorUtility.SetDirty(m_Asset);
            BlueprintDirtyGraphStore.MarkDirty(m_Asset);
            Undo.CollapseUndoOperations(undoGroup);
            // 保存编辑后的基线，UndoCoordinator 之后据此识别该资产是否被 Undo/Redo 恢复。
            BlueprintUndoCoordinator.Capture(m_Asset);
            if (!changes.IsEmpty)
            {
                // 外部缓存先于视图广播，视图异常不应阻止全局连接状态获得更新通知。
                BlueprintSideEffectRegistry.NotifyChanged(m_Asset, changes);
                GraphChanged?.Invoke(m_Asset, changes);
            }
            return BlueprintEditResult.Success(changes);
        }

        private void DisconnectInternal(BlueprintConnectionId connectionId, BlueprintChangeSet changes)
        {
            if (!m_Asset.TryFindOwnedConnectionInternal(connectionId, out BlueprintPortData owner, out BlueprintConnectionData connection))
            {
                return;
            }
            BlueprintPortReference ownerReference = owner.Reference;
            BlueprintPortReference targetReference = connection.Target;
            if (m_Asset.TryResolvePort(targetReference, out BlueprintPortData target))
            {
                // 先移除 target 的反向索引，再删除 owner 的权威连接数据。
                target.RemoveIncomingConnectionInternal(connectionId, ownerReference);
            }
            owner.RemoveOwnedConnectionInternal(connectionId);
            changes.MarkConnectionRemoved(connectionId);
        }

        private static void CollectConnectionIds(BlueprintPortData port, HashSet<BlueprintConnectionId> results)
        {
            IReadOnlyList<BlueprintConnectionData> owned = port.OwnedConnections;
            for (int i = 0; i < owned.Count; i++)
            {
                if (owned[i] != null)
                {
                    results.Add(owned[i].Id);
                }
            }
            IReadOnlyList<BlueprintIncomingConnection> incoming = port.IncomingConnections;
            for (int i = 0; i < incoming.Count; i++)
            {
                results.Add(incoming[i].ConnectionId);
            }
        }
    }
}
