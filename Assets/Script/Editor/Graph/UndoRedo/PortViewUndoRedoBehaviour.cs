using System;
using YBFramework.Bridge.Data;

namespace YBFramework.Editor.Graph
{
    public sealed class PortViewUndoRedoBehaviour : IUndoRedoBehaviour
    {
        private Action<BaseNodeData, BasePortData> m_InitializePortViewData;

        private int m_NodeID;

        private int m_PortID;

        private bool m_IsAdd;

        public void Initialize(Action<BaseNodeData, BasePortData> initializePortViewData, int nodeID, int portID, bool isAdd)
        {
            m_InitializePortViewData = initializePortViewData;
            m_NodeID = nodeID;
            m_PortID = portID;
            m_IsAdd = isAdd;
        }

        private void AddPortView(IUndoRedoRecorder undoRedoRecorder)
        {
            GraphAssetDrawer graphAssetDrawer = (GraphAssetDrawer)undoRedoRecorder;
            BaseNodeDrawer nodeDrawer = graphAssetDrawer.FindNodeDrawer(m_NodeID);
            if (nodeDrawer != null)
            {
                BasePortDrawer portDrawer = nodeDrawer.FindPortDrawer(m_PortID);
                if (portDrawer == null)
                {
                    BasePortData portData = nodeDrawer.GetNodeData().FindPortData(m_PortID);
                    if (portData != null)
                    {
                        BaseNodeData nodeData = nodeDrawer.GetNodeData();
                        portData.SetNodeData(nodeData);
                        m_InitializePortViewData?.Invoke(nodeDrawer.GetNodeData(), portData);
                        PortView portView = nodeDrawer.DrawPortView(portData);
                        nodeDrawer.GetNodeView().RefreshPortContainerDisplay();
                        portView.RevertPortViewConnections();
                    }
                }
            }
        }

        private void RemovePortView(IUndoRedoRecorder undoRedoRecorder)
        {
            GraphAssetDrawer graphAssetDrawer = (GraphAssetDrawer)undoRedoRecorder;
            NodeView nodeView = graphAssetDrawer.GetGraphView().FindNodeView(m_NodeID);
            if (nodeView != null)
            {
                PortView portView = nodeView.FindPortView(m_PortID);
                if (portView != null)
                {
                    nodeView.GetNodeDrawer().RemovePortDrawer(portView.GetPortDrawer());
                }
            }
        }

        public void Undo(IUndoRedoRecorder undoRedoRecorder)
        {
            if (m_IsAdd)
            {
                RemovePortView(undoRedoRecorder);
            }
            else
            {
                AddPortView(undoRedoRecorder);
            }
        }

        public void Redo(IUndoRedoRecorder undoRedoRecorder)
        {
            if (m_IsAdd)
            {
                AddPortView(undoRedoRecorder);
            }
            else
            {
                RemovePortView(undoRedoRecorder);
            }
        }
    }
}