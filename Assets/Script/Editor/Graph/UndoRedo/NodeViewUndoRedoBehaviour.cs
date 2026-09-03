using YBFramework.Bridge.Data;

namespace YBFramework.Editor.Graph
{
    public sealed class NodeViewUndoRedoBehaviour : IUndoRedoBehaviour
    {
        private GraphAssetDrawer m_GraphAssetDrawer;

        private int m_NodeID;

        private bool m_IsAdd;

        public void Initialize(GraphAssetDrawer graphAssetDrawer, int nodeID, bool isAdd)
        {
            m_GraphAssetDrawer = graphAssetDrawer;
            m_NodeID = nodeID;
            m_IsAdd = isAdd;
        }

        private void AddNodeView()
        {
            BaseNodeData nodeData = m_GraphAssetDrawer.GetGraphAsset().FindNodeData(m_NodeID);
            if (nodeData != null)
            {
                BaseNodeDrawer nodeDrawer = BaseNodeDrawer.Allocate(nodeData.GetType());
                if (nodeDrawer != null)
                {
                    NodeView nodeView = nodeDrawer.DrawNodeView(m_GraphAssetDrawer, nodeData);
                    m_GraphAssetDrawer.GetGraphView().AddNodeView(nodeView);
                    m_GraphAssetDrawer.AddNodeDrawer(nodeDrawer);
                    //这里会连接所有的连线
                    m_GraphAssetDrawer.RevertNodeViewConnections(nodeView);
                }
            }
        }

        private void RemoveNodeView()
        {
            NodeView nodeView = m_GraphAssetDrawer.GetGraphView().FindNodeView(m_NodeID);
            if (nodeView != null)
            {
                m_GraphAssetDrawer.GetGraphView().RemoveNodeView(nodeView);
                m_GraphAssetDrawer.RemoveNodeDrawer(nodeView.GetNodeDrawer());
            }
        }

        public void Undo()
        {
            if (m_IsAdd)
            {
                RemoveNodeView();
            }
            else
            {
                AddNodeView();
            }
        }

        public void Redo()
        {
            if (m_IsAdd)
            {
                AddNodeView();
            }
            else
            {
                RemoveNodeView();
            }
        }
    }
}