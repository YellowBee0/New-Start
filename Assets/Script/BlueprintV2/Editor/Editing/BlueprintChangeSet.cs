using System.Collections.Generic;

namespace YBFramework.BlueprintV2.Editor
{
    /// <summary>
    /// 一次业务编辑产生的最小视图变更描述。
    /// 这里只传稳定 ID，不传可能在 Undo 后失效的数据实例或 View 实例。
    /// </summary>
    public sealed class BlueprintChangeSet
    {
        private readonly HashSet<BlueprintNodeId> m_AddedNodes = new();
        private readonly HashSet<BlueprintNodeId> m_RemovedNodes = new();
        private readonly HashSet<BlueprintNodeId> m_ChangedNodes = new();
        private readonly HashSet<BlueprintPortReference> m_AddedPorts = new();
        private readonly HashSet<BlueprintPortReference> m_RemovedPorts = new();
        private readonly HashSet<BlueprintPortReference> m_ChangedPorts = new();
        private readonly HashSet<BlueprintConnectionId> m_AddedConnections = new();
        private readonly HashSet<BlueprintConnectionId> m_RemovedConnections = new();
        private readonly HashSet<BlueprintConnectionId> m_ChangedConnections = new();

        public IReadOnlyCollection<BlueprintNodeId> AddedNodes => m_AddedNodes;

        public IReadOnlyCollection<BlueprintNodeId> RemovedNodes => m_RemovedNodes;

        public IReadOnlyCollection<BlueprintNodeId> ChangedNodes => m_ChangedNodes;

        public IReadOnlyCollection<BlueprintPortReference> AddedPorts => m_AddedPorts;

        public IReadOnlyCollection<BlueprintPortReference> RemovedPorts => m_RemovedPorts;

        public IReadOnlyCollection<BlueprintPortReference> ChangedPorts => m_ChangedPorts;

        public IReadOnlyCollection<BlueprintConnectionId> AddedConnections => m_AddedConnections;

        public IReadOnlyCollection<BlueprintConnectionId> RemovedConnections => m_RemovedConnections;

        public IReadOnlyCollection<BlueprintConnectionId> ChangedConnections => m_ChangedConnections;

        public bool IsEmpty =>
            m_AddedNodes.Count == 0 &&
            m_RemovedNodes.Count == 0 &&
            m_ChangedNodes.Count == 0 &&
            m_AddedPorts.Count == 0 &&
            m_RemovedPorts.Count == 0 &&
            m_ChangedPorts.Count == 0 &&
            m_AddedConnections.Count == 0 &&
            m_RemovedConnections.Count == 0 &&
            m_ChangedConnections.Count == 0;

        internal void MarkNodeAdded(BlueprintNodeId nodeId) => m_AddedNodes.Add(nodeId);

        internal void MarkNodeRemoved(BlueprintNodeId nodeId) => m_RemovedNodes.Add(nodeId);

        internal void MarkNodeChanged(BlueprintNodeId nodeId) => m_ChangedNodes.Add(nodeId);

        internal void MarkPortAdded(BlueprintPortReference port) => m_AddedPorts.Add(port);

        internal void MarkPortRemoved(BlueprintPortReference port) => m_RemovedPorts.Add(port);

        internal void MarkPortChanged(BlueprintPortReference port) => m_ChangedPorts.Add(port);

        internal void MarkConnectionAdded(BlueprintConnectionId connectionId) => m_AddedConnections.Add(connectionId);

        internal void MarkConnectionRemoved(BlueprintConnectionId connectionId) => m_RemovedConnections.Add(connectionId);

        internal void MarkConnectionChanged(BlueprintConnectionId connectionId) => m_ChangedConnections.Add(connectionId);
    }

    /// <summary>
    /// 编辑入口的统一返回值。调用方只负责显示错误，不应自行补写模型或视图。
    /// </summary>
    public readonly struct BlueprintEditResult
    {
        private BlueprintEditResult(bool succeeded, string error, BlueprintChangeSet changes)
        {
            Succeeded = succeeded;
            Error = error;
            Changes = changes;
        }

        public bool Succeeded { get; }

        public string Error { get; }

        public BlueprintChangeSet Changes { get; }

        public static BlueprintEditResult Success(BlueprintChangeSet changes)
        {
            return new BlueprintEditResult(true, null, changes);
        }

        public static BlueprintEditResult Failure(string error)
        {
            return new BlueprintEditResult(false, error, null);
        }
    }
}
