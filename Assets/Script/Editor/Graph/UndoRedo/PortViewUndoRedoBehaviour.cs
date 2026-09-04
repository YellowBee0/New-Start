using YBFramework.Bridge.Data;

namespace YBFramework.Editor.Graph
{
    public sealed class PortViewUndoRedoBehaviour : IUndoRedoBehaviour
    {
        private GraphAssetDrawer m_GraphAssetDrawer;

        private int m_NodeID;

        private int m_PortID;

        private bool m_IsAdd;

        public void Initialize(GraphAssetDrawer graphAssetDrawer, int nodeID, int portID, bool isAdd)
        {
            m_GraphAssetDrawer = graphAssetDrawer;
            m_NodeID = nodeID;
            m_PortID = portID;
            m_IsAdd = isAdd;
        }

        private void AddPortView()
        {
            BaseNodeDrawer nodeDrawer = m_GraphAssetDrawer.FindNodeDrawer(m_NodeID);
            if (nodeDrawer != null)
            {
                m_GraphAssetDrawer.GetSO().Update();
                //TODO:这里还得恢复PortData的非序列化数据
                BasePortDrawer portDrawer = nodeDrawer.FindPortDrawer(m_PortID);
                if (portDrawer == null)
                {
                    BasePortData portData = nodeDrawer.GetNodeData().FindPortData(m_PortID);
                    if (portData != null)
                    {
                        nodeDrawer.DrawPortView(portData);
                        //TODO:这里还需要恢复PortView的连线
                        nodeDrawer.GetNodeView().RefreshPortContainerDisplay();
                    }
                }
            }
        }

        private void RemovePortView()
        {
            NodeView nodeView = m_GraphAssetDrawer.GetGraphView().FindNodeView(m_NodeID);
            if (nodeView != null)
            {
                PortView portView = nodeView.FindPortView(m_PortID);
                if (portView != null)
                {
                    nodeView.GetNodeDrawer().RemovePortDrawer(portView.GetPortDrawer());
                }
            }
        }

        public void Undo()
        {
            if (m_IsAdd)
            {
                RemovePortView();
            }
            else
            {
                AddPortView();
            }
        }

        public void Redo()
        {
            if (m_IsAdd)
            {
                AddPortView();
            }
            else
            {
                RemovePortView();
            }
        }
    }
}