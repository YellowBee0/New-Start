using System;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace YBFramework.BlueprintV2.Editor
{
    /// <summary>
    /// 节点的纯视图对象。只保存稳定 ID 和模型类型，不缓存 BlueprintNodeData 实例。
    /// </summary>
    public class BlueprintNodeView : Node
    {
        private readonly Type m_ModelType;

        public BlueprintNodeView(BlueprintNodeData node)
        {
            if (node == null)
            {
                throw new ArgumentNullException(nameof(node));
            }

            NodeId = node.Id;
            m_ModelType = node.GetType();
            Refresh(node);
        }

        public BlueprintNodeId NodeId { get; }

        public bool Matches(BlueprintNodeData node)
        {
            // 相同 ID 但类型改变时必须替换 View，确保 ViewFactory 可以重新选择正确的派生视图。
            return node != null && node.Id == NodeId && node.GetType() == m_ModelType;
        }

        public virtual void Refresh(BlueprintNodeData node)
        {
            title = node.Title;
            Rect position = GetPosition();
            position.position = node.Position;
            if (position.size == Vector2.zero)
            {
                position.size = new Vector2(160f, 80f);
            }
            SetPosition(position);
        }

        internal void AddPort(BlueprintPortView port)
        {
            if (port.direction == Direction.Input)
            {
                inputContainer.Add(port);
            }
            else
            {
                outputContainer.Add(port);
            }
            RefreshPortContainers();
        }

        internal void RemovePort(BlueprintPortView port)
        {
            if (port.parent != null)
            {
                port.RemoveFromHierarchy();
            }
            RefreshPortContainers();
        }

        internal void RefreshPortContainers()
        {
            RefreshExpandedState();
            RefreshPorts();
        }
    }
}
