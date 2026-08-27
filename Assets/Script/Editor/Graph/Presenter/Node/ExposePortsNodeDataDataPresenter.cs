using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using YBFramework.Bridge.Data;
using YBFramework.Editor.Graph.Presenter;

namespace YBFramework.Editor.Graph
{
    [RuntimeToEditor(typeof(ExposePortsNodeData))]
    public sealed class ExposePortsNodeDataDataPresenter : BaseNodeDataPresenter
    {
        private Toggle m_DirectionToggle;

        private SerializedProperty m_NodeSerializedProperty;

        public override void Initialize(BaseNodeData nodeData, SerializedProperty nodeSerializedProperty)
        {
            m_NodeSerializedProperty = nodeSerializedProperty;
            base.Initialize(nodeData, nodeSerializedProperty);
            VisualElement buttonContainer = new()
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
            buttonContainer.Add(addButton);
            buttonContainer.Add(removeButton);
            ExposePortsNodeData exposePortsNodeData = (ExposePortsNodeData)nodeData;
            m_DirectionToggle = new Toggle("输入方向")
            {
                value = exposePortsNodeData.GetIsInput()
            };
            m_DirectionToggle.RegisterValueChangedCallback(OnIsProxyInputChanged);
            m_NodeView.contentContainer.Add(m_DirectionToggle);
            m_NodeView.contentContainer.Add(buttonContainer);
        }

        private void OnAddClicked()
        {
            ExposePortsNodeData exposePortsNodeData = (ExposePortsNodeData)m_NodeData;
            //TODO:需要支持Undo
            ExposePortData exposePortData = new();
            exposePortsNodeData.AddExposePortData(exposePortData);
            exposePortsNodeData.InitializeExposePortDataView(exposePortData);
            m_NodeSerializedProperty.serializedObject.Update();
            //创建端口视图
            BasePortPresenter portPresenter = BasePortPresenter.AllocatePortPresenter(typeof(ExposePortData));
            if (portPresenter != null)
            {
                portPresenter.Initialize(exposePortData, m_NodeSerializedProperty.FindPropertyRelative(exposePortData.GetFieldName()));
                AddPortPresenter(portPresenter);
            }
            //刷新Node视图
            m_NodeView.RefreshPortContainerDisplay();
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

        private void OnIsProxyInputChanged(ChangeEvent<bool> evt)
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
            GraphPresenter graphPresenter = GraphWindow.GetInstance().GetOpenedPresenter();
            for (int i = 0; i < m_PortPresenters.Count; i++)
            {
                CustomGraphView.DisconnectAll(graphPresenter.GetGraphView(), m_PortPresenters[i].GetPortView());
                m_NodeView.RemovePortContentView(m_PortPresenters[i].GetPortContentView(), m_PortPresenters[i].GetPortData().GetDirection());
                BasePortPresenter.ReleasePortPresenter(m_PortPresenters[i]);
            }
            m_PortPresenters.Clear();
        }
    }
}