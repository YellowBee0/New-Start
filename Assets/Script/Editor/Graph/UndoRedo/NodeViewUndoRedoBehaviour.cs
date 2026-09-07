using YBFramework.Bridge.Data;

namespace YBFramework.Editor.Graph
{
    public sealed class NodeViewUndoRedoBehaviour : IUndoRedoBehaviour
    {
        private int m_NodeID;

        private bool m_IsAdd;

        public void Initialize(int nodeID, bool isAdd)
        {
            m_NodeID = nodeID;
            m_IsAdd = isAdd;
        }

        private void AddNodeView(IUndoRedoRecorder undoRedoRecorder)
        {
            GraphAssetDrawer graphAssetDrawer = (GraphAssetDrawer)undoRedoRecorder;
            BaseNodeData nodeData = graphAssetDrawer.GetGraphAsset().FindNodeData(m_NodeID);
            if (nodeData != null)
            {
                //临时处理
                nodeData.SetGraphAsset(graphAssetDrawer.GetGraphAsset());
                nodeData.SetDirtyToReinitialize();
                int portDataCount = nodeData.GetPortsDataCount();
                for (int j = 0; j < portDataCount; j++)
                {
                    BasePortData portData = nodeData.PortDataOfIndex(j);
                    portData.SetNodeData(nodeData);
                }
                //临时处理
                BaseNodeDrawer nodeDrawer = BaseNodeDrawer.Allocate(nodeData.GetType());
                if (nodeDrawer != null)
                {
                    NodeView nodeView = nodeDrawer.DrawNodeView(graphAssetDrawer, nodeData);
                    graphAssetDrawer.AddNodeDrawer(nodeDrawer);
                    //这里会连接所有的连线
                    nodeView.RevertPortViewsConnection();
                }
            }
        }

        private void RemoveNodeView(IUndoRedoRecorder undoRedoRecorder)
        {
            GraphAssetDrawer graphAssetDrawer = (GraphAssetDrawer)undoRedoRecorder;
            NodeView nodeView = graphAssetDrawer.GetGraphView().FindNodeView(m_NodeID);
            if (nodeView != null)
            {
                graphAssetDrawer.RemoveNodeDrawer(nodeView.GetNodeDrawer());
            }
        }

        public void Undo(IUndoRedoRecorder undoRedoRecorder)
        {
            if (m_IsAdd)
            {
                RemoveNodeView(undoRedoRecorder);
            }
            else
            {
                AddNodeView(undoRedoRecorder);
            }
        }

        public void Redo(IUndoRedoRecorder undoRedoRecorder)
        {
            if (m_IsAdd)
            {
                AddNodeView(undoRedoRecorder);
            }
            else
            {
                RemoveNodeView(undoRedoRecorder);
            }
        }
    }
}