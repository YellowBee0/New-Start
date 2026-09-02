using System.Collections.Generic;

namespace YBFramework.BlueprintV2
{
    public enum BlueprintValidationSeverity
    {
        Warning,
        Error
    }

    public sealed class BlueprintValidationIssue
    {
        public BlueprintValidationIssue(BlueprintValidationSeverity severity, string message)
        {
            Severity = severity;
            Message = message;
        }

        public BlueprintValidationSeverity Severity { get; }

        public string Message { get; }
    }

    public sealed class BlueprintValidationReport
    {
        private readonly List<BlueprintValidationIssue> m_Issues = new();

        public IReadOnlyList<BlueprintValidationIssue> Issues => m_Issues;

        public bool IsValid
        {
            get
            {
                for (int i = 0; i < m_Issues.Count; i++)
                {
                    if (m_Issues[i].Severity == BlueprintValidationSeverity.Error)
                    {
                        return false;
                    }
                }
                return true;
            }
        }

        internal void AddError(string message)
        {
            m_Issues.Add(new BlueprintValidationIssue(BlueprintValidationSeverity.Error, message));
        }

        internal void AddWarning(string message)
        {
            m_Issues.Add(new BlueprintValidationIssue(BlueprintValidationSeverity.Warning, message));
        }
    }

    /// <summary>
    /// 保存或执行前的数据一致性检查。校验失败不会自动修复，避免静默丢失业务连接数据。
    /// </summary>
    public static class BlueprintValidator
    {
        public static BlueprintValidationReport Validate(BlueprintAsset asset)
        {
            BlueprintValidationReport report = new BlueprintValidationReport();
            if (asset == null)
            {
                report.AddError("Blueprint asset is null.");
                return report;
            }

            asset.RebuildNonSerializedStateInternal();
            HashSet<BlueprintNodeId> nodeIds = new HashSet<BlueprintNodeId>();
            HashSet<BlueprintConnectionId> connectionIds = new HashSet<BlueprintConnectionId>();
            IReadOnlyList<BlueprintNodeData> nodes = asset.Nodes;
            for (int i = 0; i < nodes.Count; i++)
            {
                BlueprintNodeData node = nodes[i];
                if (node == null)
                {
                    report.AddError($"Node at index {i} is null.");
                    continue;
                }
                if (!node.Id.IsValid || !nodeIds.Add(node.Id))
                {
                    report.AddError($"Node at index {i} has an invalid or duplicate id: {node.Id}.");
                }

                HashSet<BlueprintPortId> portIds = new HashSet<BlueprintPortId>();
                IReadOnlyList<BlueprintPortData> ports = node.Ports;
                for (int j = 0; j < ports.Count; j++)
                {
                    BlueprintPortData port = ports[j];
                    if (port == null)
                    {
                        report.AddError($"Node {node.Id} contains a null port at index {j}.");
                        continue;
                    }
                    if (!port.Id.IsValid || !portIds.Add(port.Id))
                    {
                        report.AddError($"Node {node.Id} has an invalid or duplicate port id: {port.Id}.");
                    }
                    if (port.Capacity == BlueprintPortCapacity.Single && port.TotalConnectionCount > 1)
                    {
                        report.AddError($"Single-capacity port {port.Reference} has {port.TotalConnectionCount} connections.");
                    }
                    ValidateOwnedConnections(asset, port, connectionIds, report);
                    ValidateIncomingConnections(asset, port, report);
                }
            }
            return report;
        }

        private static void ValidateOwnedConnections(
            BlueprintAsset asset,
            BlueprintPortData owner,
            HashSet<BlueprintConnectionId> connectionIds,
            BlueprintValidationReport report)
        {
            IReadOnlyList<BlueprintConnectionData> connections = owner.OwnedConnections;
            for (int i = 0; i < connections.Count; i++)
            {
                BlueprintConnectionData connection = connections[i];
                if (connection == null)
                {
                    report.AddError($"Port {owner.Reference} contains a null owned connection.");
                    continue;
                }
                if (!connection.Id.IsValid || !connectionIds.Add(connection.Id))
                {
                    report.AddError($"Connection on port {owner.Reference} has an invalid or duplicate id: {connection.Id}.");
                }
                if (!asset.TryResolvePort(connection.Target, out BlueprintPortData target))
                {
                    report.AddError($"Connection {connection.Id} targets missing port {connection.Target}.");
                    continue;
                }
                if (!target.HasIncomingConnectionInternal(connection.Id, owner.Reference))
                {
                    // owner 的权威连接与 target 的反向索引必须成对存在。
                    report.AddError($"Connection {connection.Id} is missing its incoming reverse index on {target.Reference}.");
                }
                if (!owner.CanOwnConnection(target, out string reason))
                {
                    report.AddWarning($"Connection {connection.Id} no longer passes its port policy: {reason}");
                }
            }
        }

        private static void ValidateIncomingConnections(
            BlueprintAsset asset,
            BlueprintPortData target,
            BlueprintValidationReport report)
        {
            IReadOnlyList<BlueprintIncomingConnection> incomingConnections = target.IncomingConnections;
            for (int i = 0; i < incomingConnections.Count; i++)
            {
                BlueprintIncomingConnection incoming = incomingConnections[i];
                if (!asset.TryResolvePort(incoming.Owner, out BlueprintPortData owner))
                {
                    report.AddError($"Incoming connection {incoming.ConnectionId} references missing owner {incoming.Owner}.");
                    continue;
                }
                bool found = false;
                IReadOnlyList<BlueprintConnectionData> ownedConnections = owner.OwnedConnections;
                for (int j = 0; j < ownedConnections.Count; j++)
                {
                    BlueprintConnectionData connection = ownedConnections[j];
                    if (connection != null && connection.Id == incoming.ConnectionId && connection.Target == target.Reference)
                    {
                        found = true;
                        break;
                    }
                }
                if (!found)
                {
                    report.AddError($"Incoming connection {incoming.ConnectionId} has no matching owned connection on {incoming.Owner}.");
                }
            }
        }
    }
}
