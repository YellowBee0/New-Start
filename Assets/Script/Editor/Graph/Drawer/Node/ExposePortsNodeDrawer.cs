using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using YBFramework.Bridge.Data;

namespace YBFramework.Editor.Graph
{
    [RuntimeToEditor(typeof(ExposePortsNodeData))]
    public sealed class ExposePortsNodeDrawer : BaseNodeDrawer
    {
        private VisualElement m_ButtonContainer;

        private Toggle m_DirectionToggle;

        public ExposePortsNodeDrawer()
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
            ExposePortsNodeData exposePortsNodeData = (ExposePortsNodeData)m_NodeData;
            IReadOnlyList<BaseNodeData> nodesData = exposePortsNodeData.GetGraphAsset().GetNodesData();
            for (int i = 0; i < nodesData.Count; i++)
            {
                BaseNodeData nodeData = nodesData[i];
                if (nodeData != exposePortsNodeData && nodeData is ExposePortsNodeData proxyTargetNodeData)
                {
                    if (proxyTargetNodeData.GetIsInput() == evt.newValue)
                    {
                        m_DirectionToggle.SetValueWithoutNotify(exposePortsNodeData.GetIsInput());
                        return;
                    }
                }
            }
            exposePortsNodeData.ChangeDirection(evt.newValue);
            //TODO:清理Port Drawer，Port View
            //暂时处理
            OnRelease();
            //
        }

        protected override NodeView OnDrawNodeView(BaseNodeData nodeData)
        {
            base.OnDrawNodeView(nodeData);
            ExposePortsNodeData exposePortsNodeData = (ExposePortsNodeData)nodeData;
            m_DirectionToggle.SetValueWithoutNotify(exposePortsNodeData.GetIsInput());
            m_NodeView.contentContainer.Add(m_DirectionToggle);
            m_NodeView.contentContainer.Add(m_ButtonContainer);
            return m_NodeView;
        }

        private void OnAddClicked()
        {
            ExposePortsNodeData exposePortsNodeData = (ExposePortsNodeData)m_NodeData;
            //TODO:需要支持Undo
            ExposePortData exposePortData = new();
            exposePortsNodeData.AddExposePortData(exposePortData);
            exposePortsNodeData.InitializeExposePortDataView(exposePortData);
            //创建端口视图
            BasePortDrawer portDataPresenter = BasePortDrawer.Allocate(typeof(ExposePortData));
            if (portDataPresenter != null)
            {
                m_NodeView.AddPortView(portDataPresenter.DrawPortView(this, exposePortData));
                AddPortDrawer(portDataPresenter);
            }
            m_NodeView.RefreshPortContainerDisplay();
        }
    }
}