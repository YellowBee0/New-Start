using System;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;

namespace YBFramework.BlueprintV2.Editor
{
    /// <summary>
    /// 将 BlueprintAsset 投影为 GraphView 元素，并用稳定 ID 维护一对一映射。
    /// 普通编辑使用 ChangeSet 精确更新；Undo/Redo 使用 Reconcile 计算差异。
    /// </summary>
    public sealed class BlueprintGraphProjection
    {
        private readonly BlueprintAsset m_Asset;
        private readonly BlueprintGraphView m_GraphView;
        private readonly IBlueprintViewFactory m_Factory;
        // 字典是局部更新的核心：任何操作都可以直接定位目标 View，无需遍历并重建整个 GraphView。
        private readonly Dictionary<BlueprintNodeId, BlueprintNodeView> m_NodeViews = new();
        private readonly Dictionary<BlueprintPortReference, BlueprintPortView> m_PortViews = new();
        private readonly Dictionary<BlueprintConnectionId, BlueprintEdgeView> m_EdgeViews = new();
        // 保存 View 最后一次应用的数据 revision，Undo 后仅刷新 revision 发生变化的元素。
        private readonly Dictionary<BlueprintNodeId, int> m_NodeViewRevisions = new();
        private readonly Dictionary<BlueprintPortReference, int> m_PortViewRevisions = new();
        private readonly Dictionary<BlueprintConnectionId, int> m_EdgeViewRevisions = new();

        public BlueprintGraphProjection(
            BlueprintAsset asset,
            BlueprintGraphView graphView,
            IBlueprintViewFactory factory = null)
        {
            m_Asset = asset != null ? asset : throw new ArgumentNullException(nameof(asset));
            m_GraphView = graphView != null ? graphView : throw new ArgumentNullException(nameof(graphView));
            m_Factory = factory ?? new DefaultBlueprintViewFactory();
        }

        public bool TryGetNodeView(BlueprintNodeId nodeId, out BlueprintNodeView view)
        {
            return m_NodeViews.TryGetValue(nodeId, out view);
        }

        /// <summary>
        /// 应用正常业务操作产生的精确变更集，不进行全图扫描。
        /// </summary>
        public void Apply(BlueprintChangeSet changes)
        {
            if (changes == null || changes.IsEmpty)
            {
                return;
            }

            RunProjectionUpdate(() =>
            {
                // 必须先删除 Edge，再删除 Port/Node，防止 GraphView 中留下引用已移除端口的连线。
                foreach (BlueprintConnectionId id in changes.RemovedConnections)
                {
                    RemoveEdge(id);
                }
                foreach (BlueprintPortReference reference in changes.RemovedPorts)
                {
                    RemovePort(reference);
                }
                foreach (BlueprintNodeId id in changes.RemovedNodes)
                {
                    RemoveNode(id);
                }

                // 创建顺序与删除相反：Node -> Port -> Edge，确保 Edge 的两端 View 已存在。
                foreach (BlueprintNodeId id in changes.AddedNodes)
                {
                    UpsertNode(id);
                }
                foreach (BlueprintNodeId id in changes.ChangedNodes)
                {
                    UpsertNode(id);
                }
                foreach (BlueprintPortReference reference in changes.AddedPorts)
                {
                    UpsertPort(reference);
                }
                foreach (BlueprintPortReference reference in changes.ChangedPorts)
                {
                    UpsertPort(reference);
                }
                foreach (BlueprintConnectionId id in changes.AddedConnections)
                {
                    UpsertEdge(id);
                }
                foreach (BlueprintConnectionId id in changes.ChangedConnections)
                {
                    UpsertEdge(id);
                }
            });
        }

        /// <summary>
        /// Undo/Redo 后根据恢复完成的模型做差异同步。
        /// 方法会扫描模型以发现差异，但只增删或刷新真正变化的 View，不会 Clear GraphView。
        /// </summary>
        public void Reconcile()
        {
            m_Asset.RebuildNonSerializedStateInternal();

            Dictionary<BlueprintNodeId, BlueprintNodeData> nodes = new();
            Dictionary<BlueprintPortReference, BlueprintPortData> ports = new();
            Dictionary<BlueprintConnectionId, ConnectionRecord> connections = new();
            CollectModelState(nodes, ports, connections);

            HashSet<BlueprintNodeId> nodesToReplace = new();
            foreach (KeyValuePair<BlueprintNodeId, BlueprintNodeView> pair in m_NodeViews)
            {
                if (nodes.TryGetValue(pair.Key, out BlueprintNodeData node) && !pair.Value.Matches(node))
                {
                    nodesToReplace.Add(pair.Key);
                }
            }

            HashSet<BlueprintPortReference> portsToReplace = new();
            foreach (KeyValuePair<BlueprintPortReference, BlueprintPortView> pair in m_PortViews)
            {
                if (ports.TryGetValue(pair.Key, out BlueprintPortData port) &&
                    (!pair.Value.Matches(port) || nodesToReplace.Contains(pair.Key.NodeId)))
                {
                    portsToReplace.Add(pair.Key);
                }
            }

            RunProjectionUpdate(() =>
            {
                List<BlueprintConnectionId> staleEdges = new();
                foreach (KeyValuePair<BlueprintConnectionId, BlueprintEdgeView> pair in m_EdgeViews)
                {
                    bool exists = connections.TryGetValue(pair.Key, out ConnectionRecord record);
                    if (!exists ||
                        !pair.Value.Matches(record.Connection, record.Owner) ||
                        portsToReplace.Contains(pair.Value.Owner) ||
                        portsToReplace.Contains(pair.Value.Target))
                    {
                        // 端点或具体 View 类型变化时只替换该 Edge，不影响其余连线。
                        staleEdges.Add(pair.Key);
                    }
                }
                for (int i = 0; i < staleEdges.Count; i++)
                {
                    RemoveEdge(staleEdges[i]);
                }

                List<BlueprintPortReference> stalePorts = new();
                foreach (BlueprintPortReference reference in m_PortViews.Keys)
                {
                    if (!ports.ContainsKey(reference) || portsToReplace.Contains(reference))
                    {
                        stalePorts.Add(reference);
                    }
                }
                for (int i = 0; i < stalePorts.Count; i++)
                {
                    RemovePort(stalePorts[i]);
                }

                List<BlueprintNodeId> staleNodes = new();
                foreach (BlueprintNodeId id in m_NodeViews.Keys)
                {
                    if (!nodes.ContainsKey(id) || nodesToReplace.Contains(id))
                    {
                        staleNodes.Add(id);
                    }
                }
                for (int i = 0; i < staleNodes.Count; i++)
                {
                    RemoveNode(staleNodes[i]);
                }

                foreach (KeyValuePair<BlueprintNodeId, BlueprintNodeData> pair in nodes)
                {
                    // 结构未变且 revision 相同的节点不会调用 Refresh。
                    bool refresh = !m_NodeViewRevisions.TryGetValue(pair.Key, out int oldRevision) ||
                                   oldRevision != pair.Value.ViewRevision;
                    UpsertNode(pair.Key, refresh);
                }
                foreach (KeyValuePair<BlueprintPortReference, BlueprintPortData> pair in ports)
                {
                    bool refresh = !m_PortViewRevisions.TryGetValue(pair.Key, out int oldRevision) ||
                                   oldRevision != pair.Value.ViewRevision;
                    UpsertPort(pair.Key, refresh);
                }
                foreach (KeyValuePair<BlueprintConnectionId, ConnectionRecord> pair in connections)
                {
                    bool refresh = !m_EdgeViewRevisions.TryGetValue(pair.Key, out int oldRevision) ||
                                   oldRevision != pair.Value.Connection.ViewRevision;
                    UpsertEdge(pair.Key, refresh);
                }
            });
        }

        private void UpsertNode(BlueprintNodeId nodeId, bool refreshExisting = true)
        {
            if (!m_Asset.TryGetNode(nodeId, out BlueprintNodeData node))
            {
                RemoveNode(nodeId);
                return;
            }
            if (m_NodeViews.TryGetValue(nodeId, out BlueprintNodeView view) && !view.Matches(node))
            {
                RemoveNode(nodeId);
                view = null;
            }
            if (view == null)
            {
                view = m_Factory.CreateNode(node);
                m_NodeViews.Add(nodeId, view);
                m_GraphView.AddElement(view);
                refreshExisting = true;
            }
            if (refreshExisting)
            {
                view.Refresh(node);
            }
            m_NodeViewRevisions[nodeId] = node.ViewRevision;
        }

        private void UpsertPort(BlueprintPortReference reference, bool refreshExisting = true)
        {
            if (!m_Asset.TryResolvePort(reference, out BlueprintPortData port))
            {
                RemovePort(reference);
                return;
            }
            if (!m_NodeViews.TryGetValue(reference.NodeId, out BlueprintNodeView nodeView))
            {
                UpsertNode(reference.NodeId, false);
                m_NodeViews.TryGetValue(reference.NodeId, out nodeView);
            }
            if (nodeView == null)
            {
                return;
            }

            if (m_PortViews.TryGetValue(reference, out BlueprintPortView view) && !view.Matches(port))
            {
                // GraphView 的 Direction/Capacity 在 Port 构造时确定，签名改变只能局部替换该 Port。
                RemoveEdgesForPort(reference);
                RemovePort(reference);
                view = null;
            }
            if (view == null)
            {
                view = m_Factory.CreatePort(port);
                m_PortViews.Add(reference, view);
                m_GraphView.ConfigurePort(view);
                nodeView.AddPort(view);
                refreshExisting = true;
            }
            if (refreshExisting)
            {
                view.Refresh(port);
            }
            m_PortViewRevisions[reference] = port.ViewRevision;
        }

        private void UpsertEdge(BlueprintConnectionId connectionId, bool refreshExisting = true)
        {
            if (!m_Asset.TryFindOwnedConnectionInternal(
                    connectionId,
                    out BlueprintPortData owner,
                    out BlueprintConnectionData connection))
            {
                RemoveEdge(connectionId);
                return;
            }

            BlueprintPortReference ownerReference = owner.Reference;
            BlueprintPortReference targetReference = connection.Target;
            // 解析端点时始终重新按 ID 查询，不能使用 Undo 前缓存的数据实例。
            UpsertPort(ownerReference, false);
            UpsertPort(targetReference, false);
            if (!m_PortViews.TryGetValue(ownerReference, out BlueprintPortView ownerView) ||
                !m_PortViews.TryGetValue(targetReference, out BlueprintPortView targetView))
            {
                return;
            }

            if (m_EdgeViews.TryGetValue(connectionId, out BlueprintEdgeView view) &&
                !view.Matches(connection, ownerReference))
            {
                RemoveEdge(connectionId);
                view = null;
            }
            if (view != null)
            {
                if (refreshExisting)
                {
                    view.Refresh(connection);
                }
                m_EdgeViewRevisions[connectionId] = connection.ViewRevision;
                return;
            }

            BlueprintPortView input = ownerView.direction == Direction.Input ? ownerView : targetView;
            BlueprintPortView output = ownerView.direction == Direction.Output ? ownerView : targetView;
            if (input.direction != Direction.Input || output.direction != Direction.Output)
            {
                return;
            }

            view = m_Factory.CreateEdge(connection, ownerReference);
            view.input = input;
            view.output = output;
            input.Connect(view);
            output.Connect(view);
            // 只有模型中已经存在连接后，投影层才把真实 Edge 加入 GraphView。
            m_EdgeViews.Add(connectionId, view);
            m_GraphView.AddElement(view);
            view.Refresh(connection);
            m_EdgeViewRevisions[connectionId] = connection.ViewRevision;
        }

        private void RemoveNode(BlueprintNodeId nodeId)
        {
            if (!m_NodeViews.TryGetValue(nodeId, out BlueprintNodeView nodeView))
            {
                return;
            }

            List<BlueprintPortReference> ports = new();
            foreach (BlueprintPortReference reference in m_PortViews.Keys)
            {
                if (reference.NodeId == nodeId)
                {
                    ports.Add(reference);
                }
            }
            for (int i = 0; i < ports.Count; i++)
            {
                RemovePort(ports[i]);
            }
            m_NodeViews.Remove(nodeId);
            m_NodeViewRevisions.Remove(nodeId);
            m_GraphView.RemoveElement(nodeView);
        }

        private void RemovePort(BlueprintPortReference reference)
        {
            if (!m_PortViews.TryGetValue(reference, out BlueprintPortView portView))
            {
                return;
            }
            RemoveEdgesForPort(reference);
            m_PortViews.Remove(reference);
            m_PortViewRevisions.Remove(reference);
            if (m_NodeViews.TryGetValue(reference.NodeId, out BlueprintNodeView nodeView))
            {
                nodeView.RemovePort(portView);
            }
            else
            {
                portView.RemoveFromHierarchy();
            }
        }

        private void RemoveEdgesForPort(BlueprintPortReference reference)
        {
            List<BlueprintConnectionId> connections = new();
            foreach (KeyValuePair<BlueprintConnectionId, BlueprintEdgeView> pair in m_EdgeViews)
            {
                if (pair.Value.Owner == reference || pair.Value.Target == reference)
                {
                    connections.Add(pair.Key);
                }
            }
            for (int i = 0; i < connections.Count; i++)
            {
                RemoveEdge(connections[i]);
            }
        }

        private void RemoveEdge(BlueprintConnectionId connectionId)
        {
            if (!m_EdgeViews.TryGetValue(connectionId, out BlueprintEdgeView edge))
            {
                return;
            }
            edge.input?.Disconnect(edge);
            edge.output?.Disconnect(edge);
            m_EdgeViews.Remove(connectionId);
            m_EdgeViewRevisions.Remove(connectionId);
            m_GraphView.RemoveElement(edge);
        }

        private void CollectModelState(
            Dictionary<BlueprintNodeId, BlueprintNodeData> nodes,
            Dictionary<BlueprintPortReference, BlueprintPortData> ports,
            Dictionary<BlueprintConnectionId, ConnectionRecord> connections)
        {
            IReadOnlyList<BlueprintNodeData> nodeData = m_Asset.Nodes;
            for (int i = 0; i < nodeData.Count; i++)
            {
                BlueprintNodeData node = nodeData[i];
                if (node == null || nodes.ContainsKey(node.Id))
                {
                    continue;
                }
                nodes.Add(node.Id, node);

                IReadOnlyList<BlueprintPortData> portData = node.Ports;
                for (int j = 0; j < portData.Count; j++)
                {
                    BlueprintPortData port = portData[j];
                    if (port == null || ports.ContainsKey(port.Reference))
                    {
                        continue;
                    }
                    ports.Add(port.Reference, port);

                    IReadOnlyList<BlueprintConnectionData> ownedConnections = port.OwnedConnections;
                    for (int k = 0; k < ownedConnections.Count; k++)
                    {
                        BlueprintConnectionData connection = ownedConnections[k];
                        if (connection != null && !connections.ContainsKey(connection.Id))
                        {
                            connections.Add(connection.Id, new ConnectionRecord(port.Reference, connection));
                        }
                    }
                }
            }
        }

        private void RunProjectionUpdate(Action update)
        {
            // Controller 在此期间忽略 GraphView 回调，防止“模型驱动的视图变化”被误判为用户操作。
            m_GraphView.BeginProjectionUpdate();
            try
            {
                update();
            }
            finally
            {
                m_GraphView.EndProjectionUpdate();
            }
        }

        private sealed class ConnectionRecord
        {
            public ConnectionRecord(BlueprintPortReference owner, BlueprintConnectionData connection)
            {
                Owner = owner;
                Connection = connection;
            }

            public BlueprintPortReference Owner { get; }

            public BlueprintConnectionData Connection { get; }
        }
    }
}
