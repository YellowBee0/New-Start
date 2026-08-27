using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using YBFramework.Bridge.Data;

namespace YBFramework.Editor.Graph
{
    [RuntimeToEditor(typeof(SubNodeDataBridge))]
    public sealed class SubNodeBridgePresenter : BaseNodePresenter
    {
        private Toggle m_IsProxyInputToggle;

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
            SubNodeDataBridge proxyHelperNodeData = (SubNodeDataBridge)nodeData;
            m_IsProxyInputToggle = new Toggle("只连接输入")
            {
                value = proxyHelperNodeData.IsInputPortValid
            };
            m_IsProxyInputToggle.RegisterValueChangedCallback(OnIsProxyInputChanged);
            m_NodeView.contentContainer.Add(m_IsProxyInputToggle);
            m_NodeView.contentContainer.Add(buttonContainer);
        }

        private void OnAddClicked()
        {
            SubNodeDataBridge proxyHelperNodeData = (SubNodeDataBridge)m_NodeData;
            //TODO:需要支持Undo
            SubPortDataBridge proxyHelperPortData = new();
            proxyHelperNodeData.AddSubPortDataBridge(proxyHelperPortData);
            m_NodeSerializedProperty.serializedObject.Update();
            //创建端口视图
            BasePortPresenter portPresenter = BasePortPresenter.AllocatePortPresenter(typeof(SubPortDataBridge));
            if (portPresenter != null)
            {
                portPresenter.Initialize(proxyHelperPortData, m_NodeSerializedProperty.FindPropertyRelative(proxyHelperPortData.GetFieldName()));
                AddPortPresenter(portPresenter);
            }
            //刷新Node视图
            m_NodeView.RefreshPortContainerDisplay();
        }

        private void OnRemoveClicked()
        {
            Debug.Log("开发中");
        }

        private void OnIsProxyInputChanged(ChangeEvent<bool> evt)
        {
            SubNodeDataBridge bindNodeData = (SubNodeDataBridge)m_NodeData;
            IReadOnlyList<BaseNodeData> nodeData = bindNodeData.GetGraphAsset().GetNodesData();
            for (int i = 0; i < nodeData.Count; i++)
            {
                BaseNodeData baseNodeData = nodeData[i];
                if (baseNodeData != bindNodeData && baseNodeData is SubNodeDataBridge proxyTargetNodeData)
                {
                    if (proxyTargetNodeData.IsInputPortValid == evt.newValue)
                    {
                        m_IsProxyInputToggle.SetValueWithoutNotify(bindNodeData.IsInputPortValid);
                        return;
                    }
                }
            }
            bindNodeData.IsInputPortValid = evt.newValue;
            //TODO:移除所有端口，包括视图上的端口，连线；数据上的连线数据
        }
    }
}