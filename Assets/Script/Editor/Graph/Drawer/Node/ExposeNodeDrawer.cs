using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using YBFramework.Bridge.Data;

namespace YBFramework.Editor.Graph
{
    [RuntimeToEditor(typeof(ExposeNodeData))]
    public sealed class ExposeNodeDrawer : BaseNodeDrawer
    {
        private readonly VisualElement m_ButtonContainer;

        private readonly Toggle m_DirectionToggle;

        public ExposeNodeDrawer()
        {
            m_ButtonContainer = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Row
                }
            };
            Button addButton = new(OnAddClicked)
            {
                text = "添加"
            };
            Button removeButton = new(OnRemoveClicked)
            {
                text = "删除"
            };
            m_ButtonContainer.Add(addButton);
            m_ButtonContainer.Add(removeButton);
            m_DirectionToggle = new Toggle("输入方向");
            m_DirectionToggle.RegisterValueChangedCallback(OnDirectionChanged);
        }

        public void RefreshNodeView()
        {
            ClearPortDrawers();
            m_DirectionToggle.SetValueWithoutNotify(((ExposeNodeData)m_NodeData).GetIsInput());
            DrawPortViews();
        }
        
        private void OnAddClicked()
        {
            ExposeNodeData exposeNodeData = (ExposeNodeData)m_NodeData;
            m_GraphAssetDrawer.ModifyGraphAsset("Expose node data add expose port data");
            //TODO:封装一个函数，创建节点或者端口并且自动调用他们的InitializeSerializedData
            ExposePortData exposePortData = new();
            exposePortData.InitializeSerializedData();
            exposeNodeData.AddExposePortData(exposePortData);
            exposeNodeData.InitializeExposePortDataView(exposePortData);
            //记录Undo行为
            PortViewUndoRedoBehaviour portViewUndoRedo = IUndoRedoBehaviour.Allocate<PortViewUndoRedoBehaviour>();
            portViewUndoRedo.Initialize(m_GraphAssetDrawer, m_NodeData.GetNodeID(), exposePortData.GetPortID(), true);
            m_GraphAssetDrawer.PushUndoRedoBehaviour(portViewUndoRedo);
            //创建端口视图
            DrawPortView(exposePortData);
            m_NodeView.RefreshPortContainerDisplay();
            m_GraphAssetDrawer.ApplyModifyGraphAsset();
        }
        
        private void OnRemoveClicked()
        {
            Debug.Log("开发中");
            //删除后需要刷新端口名
            /*
            ((ExposePortsNodeData)m_NodeData).RefreshExposePortDataName();
            for (int i = 0; i < m_PortPresenters.Count; i++)
            {
                m_PortPresenters[i].GetPortView().portName = m_PortPresenters[i].GetPortData().GetPortName();
            }
            */
        }

        private void OnDirectionChanged(ChangeEvent<bool> evt)
        {
            ExposeNodeData exposeNodeData = (ExposeNodeData)m_NodeData;
            IReadOnlyList<BaseNodeData> nodesData = exposeNodeData.GetGraphAsset().GetNodesData();
            for (int i = 0; i < nodesData.Count; i++)
            {
                BaseNodeData nodeData = nodesData[i];
                if (nodeData != exposeNodeData && nodeData is ExposeNodeData otherExposeNodeData)
                {
                    if (otherExposeNodeData.GetIsInput() == evt.newValue)
                    {
                        m_DirectionToggle.SetValueWithoutNotify(exposeNodeData.GetIsInput());
                        return;
                    }
                }
            }
            m_GraphAssetDrawer.ModifyGraphAsset("Change expose port data direction");
            exposeNodeData.ChangeDirection(evt.newValue);
            ClearPortDrawers();
            //记录Undo
            ExposeNodeDirectionUndoRedoBehaviour exposeNodeDirectionUndoRedo = IUndoRedoBehaviour.Allocate<ExposeNodeDirectionUndoRedoBehaviour>();
            exposeNodeDirectionUndoRedo.Initialize(m_GraphAssetDrawer, m_NodeData.GetNodeID());
            m_GraphAssetDrawer.PushUndoRedoBehaviour(exposeNodeDirectionUndoRedo);
            m_GraphAssetDrawer.ApplyModifyGraphAsset();
        }

        protected override void OnDrawNodeView()
        {
            base.OnDrawNodeView();
            ExposeNodeData exposeNodeData = (ExposeNodeData)m_NodeData;
            m_DirectionToggle.SetValueWithoutNotify(exposeNodeData.GetIsInput());
            m_NodeView.contentContainer.Add(m_DirectionToggle);
            m_NodeView.contentContainer.Add(m_ButtonContainer);
        }

        protected override void OnRelease()
        {
            base.OnRelease();
            m_NodeView.contentContainer.Remove(m_DirectionToggle);
            m_NodeView.contentContainer.Remove(m_ButtonContainer);
        }
    }
}