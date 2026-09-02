using System;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace YBFramework.BlueprintV2.Editor
{
    /// <summary>
    /// GraphView 容器，只负责收集用户意图，不直接持久化任何蓝图数据。
    /// </summary>
    public sealed class BlueprintGraphView : GraphView
    {
        private readonly List<Port> m_CompatiblePorts = new();
        private readonly ConnectionDropListener m_DropListener;

        public BlueprintGraphView()
        {
            m_DropListener = new ConnectionDropListener(this);
            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());

            GridBackground grid = new GridBackground();
            grid.StretchToParentSize();
            Insert(0, grid);
            style.flexGrow = 1f;
        }

        internal event Action<BlueprintPortReference, BlueprintPortReference> ConnectionRequested;

        internal Func<BlueprintPortReference, BlueprintPortReference, bool> CanConnect { get; set; }

        internal bool IsApplyingProjection { get; private set; }

        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            m_CompatiblePorts.Clear();
            if (startPort is not BlueprintPortView start)
            {
                return m_CompatiblePorts;
            }

            foreach (Port candidate in ports)
            {
                // 兼容性由模型侧 EditService 判断，避免 View 与业务端口规则产生两套实现。
                if (candidate is BlueprintPortView candidateView &&
                    candidateView != start &&
                    (CanConnect?.Invoke(start.PortReference, candidateView.PortReference) ?? false))
                {
                    m_CompatiblePorts.Add(candidateView);
                }
            }
            return m_CompatiblePorts;
        }

        internal void ConfigurePort(BlueprintPortView port)
        {
            port.AttachConnector(m_DropListener);
        }

        internal void BeginProjectionUpdate()
        {
            IsApplyingProjection = true;
        }

        internal void EndProjectionUpdate()
        {
            IsApplyingProjection = false;
        }

        private void RequestConnection(Edge edge)
        {
            if (edge?.input is not BlueprintPortView input || edge.output is not BlueprintPortView output)
            {
                return;
            }
            // EdgeConnector 传入的 edge 只是拖拽候选线。这里只发送连接请求，
            // 真正的 Edge 必须等模型写入成功后由 Projection 创建。
            ConnectionRequested?.Invoke(output.PortReference, input.PortReference);
        }

        private sealed class ConnectionDropListener : IEdgeConnectorListener
        {
            private readonly BlueprintGraphView m_GraphView;

            public ConnectionDropListener(BlueprintGraphView graphView)
            {
                m_GraphView = graphView;
            }

            public void OnDropOutsidePort(Edge edge, Vector2 position)
            {
            }

            public void OnDrop(GraphView graphView, Edge edge)
            {
                m_GraphView.RequestConnection(edge);
            }
        }
    }
}
