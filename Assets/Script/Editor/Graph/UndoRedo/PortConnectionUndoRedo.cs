using UnityEditor.Experimental.GraphView;

namespace YBFramework.Editor.Graph
{
    public sealed class PortConnectionUndoRedo : UndoRedo
    {
        private GraphAssetPresenter m_GraphAssetPresenter;

        private BasePortDataPresenter m_FirstPortDataPresenter;

        private BasePortDataPresenter m_SecondPortDataPresenter;

        private Edge m_Connection;

        public void Initialize(GraphAssetPresenter graphAssetPresenter, BasePortDataPresenter firstPortDataPresenter, BasePortDataPresenter secondPortDataPresenter, Edge connection)
        {
            m_GraphAssetPresenter = graphAssetPresenter;
            m_FirstPortDataPresenter = firstPortDataPresenter;
            m_SecondPortDataPresenter = secondPortDataPresenter;
            m_Connection = connection;
        }

        public override void Undo()
        {
            UndoRedo();
        }

        public override void Redo()
        {
            UndoRedo();
        }

        private void UndoRedo()
        {
            //初始化时m_Connection为null，代表这一次记录是删除连线的记录，Undo就恢复连线，Redo就删除连线；反之就是连接记录，Undo删除，Redo恢复。
            if (m_Connection == null)
            {
                m_Connection = m_FirstPortDataPresenter.GetPortView().ConnectTo(m_SecondPortDataPresenter.GetPortView());
                m_GraphAssetPresenter.GetGraphView().AddElement(m_Connection);
            }
            else
            {
                m_FirstPortDataPresenter.GetPortView().Disconnect(m_Connection);
                m_SecondPortDataPresenter.GetPortView().Disconnect(m_Connection);
                m_GraphAssetPresenter.GetGraphView().RemoveElement(m_Connection);
                m_Connection = null;
            }
        }
    }
}