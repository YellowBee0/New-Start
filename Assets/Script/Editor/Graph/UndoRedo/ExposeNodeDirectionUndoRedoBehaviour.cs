namespace YBFramework.Editor.Graph
{
    public sealed class ExposeNodeDirectionUndoRedoBehaviour : IUndoRedoBehaviour
    {
        private GraphAssetDrawer m_GraphAssetDrawer;

        private int m_NodeID;

        public void Initialize(GraphAssetDrawer graphAssetDrawer, int nodeID)
        {
            m_GraphAssetDrawer = graphAssetDrawer;
            m_NodeID = nodeID;
        }
        
        private void RefreshExposeNodeView()
        {
            BaseNodeDrawer nodeDrawer = m_GraphAssetDrawer.FindNodeDrawer(m_NodeID);
            if (nodeDrawer is ExposeNodeDrawer exposePortsNodeDrawer)
            {
                exposePortsNodeDrawer.RefreshNodeView();
            }
        }

        public void Undo()
        {
            RefreshExposeNodeView();
        }

        public void Redo()
        {
            RefreshExposeNodeView();
        }
    }
}