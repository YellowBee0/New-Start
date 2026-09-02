using System;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace YBFramework.BlueprintV2.Editor
{
    /// <summary>
    /// 将 GraphView 的用户操作翻译成 EditService 命令。它不直接创建、删除或修改业务 View。
    /// </summary>
    public sealed class BlueprintGraphController : IDisposable
    {
        private readonly BlueprintGraphView m_GraphView;
        private readonly BlueprintGraphProjection m_Projection;
        private readonly BlueprintEditService m_EditService;
        private readonly HashSet<BlueprintNodeId> m_PendingMovedNodes = new();
        private bool m_MoveFlushScheduled;
        private bool m_Disposed;

        public BlueprintGraphController(
            BlueprintGraphView graphView,
            BlueprintGraphProjection projection,
            BlueprintEditService editService)
        {
            m_GraphView = graphView ?? throw new ArgumentNullException(nameof(graphView));
            m_Projection = projection ?? throw new ArgumentNullException(nameof(projection));
            m_EditService = editService ?? throw new ArgumentNullException(nameof(editService));

            m_GraphView.graphViewChanged += OnGraphViewChanged;
            m_GraphView.ConnectionRequested += OnConnectionRequested;
            m_GraphView.CanConnect = CanConnect;
        }

        public void Dispose()
        {
            if (m_Disposed)
            {
                return;
            }
            m_Disposed = true;
            m_GraphView.graphViewChanged -= OnGraphViewChanged;
            m_GraphView.ConnectionRequested -= OnConnectionRequested;
            m_GraphView.CanConnect = null;
            m_PendingMovedNodes.Clear();
        }

        private bool CanConnect(BlueprintPortReference first, BlueprintPortReference second)
        {
            return m_EditService.CanConnect(first, second, out _);
        }

        private void OnConnectionRequested(BlueprintPortReference first, BlueprintPortReference second)
        {
            BlueprintEditResult result = m_EditService.Connect(first, second);
            if (!result.Succeeded)
            {
                Debug.LogWarning(result.Error, m_EditService.Asset);
            }
        }

        private GraphViewChange OnGraphViewChanged(GraphViewChange change)
        {
            if (m_Disposed || m_GraphView.IsApplyingProjection)
            {
                return change;
            }

            HandleRemovals(ref change);
            HandleMoves(change);
            return change;
        }

        private void HandleRemovals(ref GraphViewChange change)
        {
            if (change.elementsToRemove == null || change.elementsToRemove.Count == 0)
            {
                return;
            }

            HashSet<BlueprintNodeId> nodes = new();
            HashSet<BlueprintConnectionId> connections = new();
            for (int i = change.elementsToRemove.Count - 1; i >= 0; i--)
            {
                switch (change.elementsToRemove[i])
                {
                    case BlueprintNodeView node:
                        nodes.Add(node.NodeId);
                        change.elementsToRemove.RemoveAt(i);
                        break;
                    case BlueprintEdgeView edge:
                        connections.Add(edge.ConnectionId);
                        change.elementsToRemove.RemoveAt(i);
                        break;
                }
            }

            // 移除 GraphView 原本要执行的删除项。模型修改成功后，Projection 会局部删除对应 View，
            // 从而保证模型是唯一事实来源，也避免同一个元素被 GraphView 删除两次。
            if (nodes.Count == 0 && connections.Count == 0)
            {
                return;
            }

            BlueprintEditResult result = m_EditService.RemoveElements(nodes, connections);
            if (!result.Succeeded)
            {
                Debug.LogWarning(result.Error, m_EditService.Asset);
            }
        }

        private void HandleMoves(GraphViewChange change)
        {
            if (change.movedElements == null || change.movedElements.Count == 0)
            {
                return;
            }
            for (int i = 0; i < change.movedElements.Count; i++)
            {
                if (change.movedElements[i] is BlueprintNodeView node)
                {
                    m_PendingMovedNodes.Add(node.NodeId);
                }
            }
            if (m_PendingMovedNodes.Count == 0 || m_MoveFlushScheduled)
            {
                return;
            }

            m_MoveFlushScheduled = true;
            // graphViewChanged 发生时 GraphView 还未把最终位置写入 Node View。
            // 延迟到下一次 UI 调度再读取位置，可避免 Projection 刷新后 GraphView 又叠加一次 moveDelta。
            m_GraphView.schedule.Execute(FlushMovedNodes);
        }

        private void FlushMovedNodes()
        {
            m_MoveFlushScheduled = false;
            if (m_Disposed)
            {
                m_PendingMovedNodes.Clear();
                return;
            }

            Dictionary<BlueprintNodeId, Vector2> positions = new();
            foreach (BlueprintNodeId nodeId in m_PendingMovedNodes)
            {
                if (m_Projection.TryGetNodeView(nodeId, out BlueprintNodeView view))
                {
                    positions[nodeId] = view.GetPosition().position;
                }
            }
            m_PendingMovedNodes.Clear();
            if (positions.Count > 0)
            {
                BlueprintEditResult result = m_EditService.SetNodePositions(positions);
                if (!result.Succeeded)
                {
                    Debug.LogWarning(result.Error, m_EditService.Asset);
                }
            }
        }
    }
}
