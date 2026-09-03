namespace YBFramework.Editor.Graph
{
    public sealed class ConnectionUndoRedoBehaviour : IUndoRedoBehaviour
    {
        private GraphAssetDrawer m_GraphAssetDrawer;

        private int m_FromNodeID;

        private int m_FromPortID;

        private int m_ToNodeID;

        private int m_ToPortID;

        private bool m_IsConnect;

        public void Initialize(GraphAssetDrawer graphAssetDrawer, int fromNodeID, int fromPortID, int toNodeID, int toPortID, bool isConnect)
        {
            m_GraphAssetDrawer = graphAssetDrawer;
            m_FromNodeID = fromNodeID;
            m_FromPortID = fromPortID;
            m_ToNodeID = toNodeID;
            m_ToPortID = toPortID;
            m_IsConnect = isConnect;
        }

        private void Connect()
        {
            PortView fromPortView = null;
            NodeView fromNodeView = m_GraphAssetDrawer.GetGraphView().FindNodeView(m_FromNodeID);
            if (fromNodeView != null)
            {
                fromPortView = fromNodeView.FindPortView(m_FromPortID);
            }
            if (fromPortView != null)
            {
                PortView toPortView = null;
                NodeView toNodeView = m_GraphAssetDrawer.GetGraphView().FindNodeView(m_ToNodeID);
                if (toNodeView != null)
                {
                    toPortView = toNodeView.FindPortView(m_ToPortID);
                }
                if (toPortView != null)
                {
                    //正常来说这两个PortView并没有连接过对方，但是现在在一次一个节点的时候，会连同节点下的端口一起移除，连线也会断开。
                    //但是Undo行为只保存了NodeViewUndoRedoBehaviour，连线Undo并没有记录，就会出现：
                    //1、先移除一条连线，再移除整个节点：Undo的时候恢复整个节点和连线（先移除的连线也会存在，因为Undo恢复了端口的数据），然后恢复先移除的连线（原本就恢复了，这里又恢复一次）
                    //2、先添加一条连线，再移除整个节点：Undo的时候恢复整个节点和连线（先添加的连线不存在，因为Undo恢复数据后并没有这条连线），然后移除之前添加的连线（并不能找到连线）
                    if (fromPortView.FindConnection(toPortView) != null)
                    {
                        return;
                    }
                    EdgeView edgeView = fromPortView.ConnectTo<EdgeView>(toPortView);
                    edgeView.SetConnectDirection(fromPortView, toPortView);
                    m_GraphAssetDrawer.GetGraphView().AddElement(edgeView);
                }
            }
        }

        private void Disconnect()
        {
            PortView fromPortView = null;
            NodeView fromNodeView = m_GraphAssetDrawer.GetGraphView().FindNodeView(m_FromNodeID);
            if (fromNodeView != null)
            {
                fromPortView = fromNodeView.FindPortView(m_FromPortID);
            }
            if (fromPortView != null)
            {
                PortView toPortView = null;
                NodeView toNodeView = m_GraphAssetDrawer.GetGraphView().FindNodeView(m_ToNodeID);
                if (toNodeView != null)
                {
                    toPortView = toNodeView.FindPortView(m_ToPortID);
                }
                if (toPortView != null)
                {
                    CustomGraphView.Disconnect(fromPortView, toPortView, m_GraphAssetDrawer.GetGraphView());
                }
            }
        }


        public void Undo()
        {
            if (m_IsConnect)
            {
                Disconnect();
            }
            else
            {
                Connect();
            }
        }

        public void Redo()
        {
            if (m_IsConnect)
            {
                Connect();
            }
            else
            {
                Disconnect();
            }
        }
    }
}