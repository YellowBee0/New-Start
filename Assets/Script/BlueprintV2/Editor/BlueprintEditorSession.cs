using System;
using UnityEditor;

namespace YBFramework.BlueprintV2.Editor
{
    /// <summary>
    /// 一个打开的蓝图编辑会话，组合 EditService、Controller、Projection 和 GraphView 的生命周期。
    /// 多个会话打开同一资产时，会通过静态变更事件同步各自的局部视图。
    /// </summary>
    public sealed class BlueprintEditorSession : IDisposable
    {
        private readonly BlueprintGraphProjection m_Projection;
        private readonly BlueprintGraphController m_Controller;
        private bool m_Disposed;

        public BlueprintEditorSession(BlueprintAsset asset, IBlueprintViewFactory viewFactory = null)
        {
            Asset = asset != null ? asset : throw new ArgumentNullException(nameof(asset));
            EditService = new BlueprintEditService(Asset);
            GraphView = new BlueprintGraphView();
            m_Projection = new BlueprintGraphProjection(Asset, GraphView, viewFactory);
            m_Controller = new BlueprintGraphController(GraphView, m_Projection, EditService);

            BlueprintEditService.GraphChanged += OnGraphChanged;
            BlueprintUndoCoordinator.GraphRestored += OnGraphRestored;
            m_Projection.Reconcile();
        }

        public BlueprintAsset Asset { get; }

        public BlueprintEditService EditService { get; }

        public BlueprintGraphView GraphView { get; }

        public void Dispose()
        {
            if (m_Disposed)
            {
                return;
            }
            m_Disposed = true;
            BlueprintEditService.GraphChanged -= OnGraphChanged;
            BlueprintUndoCoordinator.GraphRestored -= OnGraphRestored;
            m_Controller.Dispose();
        }

        private void OnGraphChanged(BlueprintAsset asset, BlueprintChangeSet changes)
        {
            if (!m_Disposed && asset == Asset)
            {
                m_Projection.Apply(changes);
            }
        }

        private void OnGraphRestored(BlueprintAsset asset, UndoRedoInfo info)
        {
            if (!m_Disposed && asset == Asset)
            {
                // Undo 没有原业务 ChangeSet，只能对当前模型与稳定 ID 字典做差异对齐。
                // Reconcile 会扫描模型，但不会 Clear 或重建整个 GraphView。
                m_Projection.Reconcile();
            }
        }
    }
}
