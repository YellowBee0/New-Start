using UnityEngine;

namespace YBFramework.Editor.Graph
{
    public sealed class NodeViewPositionUndoRedoBehaviour : IUndoRedoBehaviour
    {
        private int m_NodeID;

        private Vector2 m_MoveDelta;

        public void Initialize(int nodeID, Vector2 moveDelta)
        {
            m_NodeID = nodeID;
            m_MoveDelta = moveDelta;
        }

        public void Undo(IUndoRedoRecorder undoRedoRecorder)
        {
            GraphAssetDrawer graphAssetDrawer = (GraphAssetDrawer)undoRedoRecorder;
            NodeView nodeView = graphAssetDrawer.GetGraphView().FindNodeView(m_NodeID);
            if (nodeView != null)
            {
                Rect oldPosition = nodeView.GetPosition();
                Rect newPosition = new(oldPosition.position - m_MoveDelta, oldPosition.size);
                nodeView.SetPosition(newPosition);
            }
        }

        public void Redo(IUndoRedoRecorder undoRedoRecorder)
        {
            GraphAssetDrawer graphAssetDrawer = (GraphAssetDrawer)undoRedoRecorder;
            NodeView nodeView = graphAssetDrawer.GetGraphView().FindNodeView(m_NodeID);
            if (nodeView != null)
            {
                Rect oldPosition = nodeView.GetPosition();
                Rect newPosition = new(oldPosition.position + m_MoveDelta, oldPosition.size);
                nodeView.SetPosition(newPosition);
            }
        }
    }
}