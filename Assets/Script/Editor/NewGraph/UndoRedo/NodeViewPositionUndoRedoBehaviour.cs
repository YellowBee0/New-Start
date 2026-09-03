using UnityEngine;

namespace YBFramework.Editor.NewGraph
{
    public sealed class NodeViewPositionUndoRedoBehaviour : IUndoRedoBehaviour
    {
        private GraphAssetDrawer m_GraphAssetDrawer;

        private int m_NodeID;

        private Vector2 m_MoveDelta;

        public void Initialize(GraphAssetDrawer graphAssetDrawer, int nodeID, Vector2 moveDelta)
        {
            m_GraphAssetDrawer = graphAssetDrawer;
            m_NodeID = nodeID;
            m_MoveDelta = moveDelta;
        }

        public void Undo()
        {
            NodeView nodeView = m_GraphAssetDrawer.GetGraphView().FindNodeView(m_NodeID);
            if (nodeView != null)
            {
                Rect oldPosition = nodeView.GetPosition();
                Rect newPosition = new(oldPosition.position - m_MoveDelta, oldPosition.size);
                nodeView.SetPosition(newPosition);
            }
        }

        public void Redo()
        {
            NodeView nodeView = m_GraphAssetDrawer.GetGraphView().FindNodeView(m_NodeID);
            if (nodeView != null)
            {
                Rect oldPosition = nodeView.GetPosition();
                Rect newPosition = new(oldPosition.position + m_MoveDelta, oldPosition.size);
                nodeView.SetPosition(newPosition);
            }
        }
    }
}