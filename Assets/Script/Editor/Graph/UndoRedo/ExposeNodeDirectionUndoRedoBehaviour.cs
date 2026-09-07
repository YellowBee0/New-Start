namespace YBFramework.Editor.Graph
{
    public sealed class ExposeNodeDirectionUndoRedoBehaviour : IUndoRedoBehaviour
    {
        private int m_NodeID;

        public void Initialize(int nodeID)
        {
            m_NodeID = nodeID;
        }

        private void RefreshExposeNodeView(IUndoRedoRecorder undoRedoRecorder)
        {
            GraphAssetDrawer graphAssetDrawer = (GraphAssetDrawer)undoRedoRecorder;
            BaseNodeDrawer nodeDrawer = graphAssetDrawer.FindNodeDrawer(m_NodeID);
            if (nodeDrawer is ExposeNodeDrawer exposePortsNodeDrawer)
            {
                exposePortsNodeDrawer.RefreshNodeView();
            }
        }

        public void Undo(IUndoRedoRecorder undoRedoRecorder)
        {
            RefreshExposeNodeView(undoRedoRecorder);
        }

        public void Redo(IUndoRedoRecorder undoRedoRecorder)
        {
            RefreshExposeNodeView(undoRedoRecorder);
        }
    }
}