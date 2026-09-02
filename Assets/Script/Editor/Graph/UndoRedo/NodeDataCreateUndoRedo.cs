using YBFramework.Bridge.Data;

namespace YBFramework.Editor.Graph
{
    public sealed class NodeDataCreateUndoRedo : UndoRedo
    {
        private GraphAssetPresenter m_GraphAssetPresenter;

        private BaseNodeDataPresenter m_NodeDataPresenter;

        private int m_NodeID;

        public void Initialize(GraphAssetPresenter graphAssetPresenter, BaseNodeDataPresenter nodeDataPresenter, int nodeID)
        {
            m_GraphAssetPresenter = graphAssetPresenter;
            m_NodeDataPresenter = nodeDataPresenter;
            m_NodeID = nodeID;
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
            if (m_NodeDataPresenter != null)
            {
                m_GraphAssetPresenter.RemoveNodeDataPresenter(m_NodeDataPresenter);
                m_NodeDataPresenter = null;
            }
            else
            {
                BaseNodeData nodeData = m_GraphAssetPresenter.GetGraphAsset().FindNodeData(m_NodeID);
                if (nodeData != null)
                {
                    m_NodeDataPresenter = BaseNodeDataPresenter.AllocateNodePresenter(nodeData.GetType());
                    if (m_NodeDataPresenter != null)
                    {
                        m_NodeDataPresenter.Initialize(m_GraphAssetPresenter, nodeData, m_GraphAssetPresenter.GetNodeSerializedProperty(nodeData));
                        m_GraphAssetPresenter.AddNodeDataPresenter(m_NodeDataPresenter);
                    }
                }
            }
        }
    }
}