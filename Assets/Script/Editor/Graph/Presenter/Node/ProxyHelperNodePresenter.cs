using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using YBFramework.Bridge.Data;

namespace YBFramework.Editor.Graph
{
    [RuntimeToEditor(typeof(ProxyHelperNodeData))]
    public sealed class ProxyHelperNodePresenter : BaseNodePresenter
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
            ProxyHelperNodeData proxyHelperNodeData = (ProxyHelperNodeData)nodeData;
            m_IsProxyInputToggle = new Toggle("只连接输入")
            {
                value = proxyHelperNodeData.IsInputPortsProxyHelper
            };
            m_IsProxyInputToggle.RegisterValueChangedCallback(OnIsProxyInputChanged);
            m_NodeView.contentContainer.Add(m_IsProxyInputToggle);
            m_NodeView.contentContainer.Add(buttonContainer);
        }

        private void OnAddClicked()
        {
            ProxyHelperNodeData proxyHelperNodeData = (ProxyHelperNodeData)m_NodeData;
            //TODO:需要支持Undo
            ProxyHelperPortData proxyHelperPortData = BaseNodeData.CreatePortData<ProxyHelperPortData>();
            proxyHelperNodeData.AddProxyHelperPortData(proxyHelperPortData);
            m_NodeSerializedProperty.serializedObject.Update();
            //创建端口视图
            BasePortPresenter portPresenter = BasePortPresenter.AllocatePortPresenter(typeof(ProxyHelperPortData));
            if (portPresenter != null)
            {
                portPresenter.Initialize(proxyHelperPortData, m_NodeSerializedProperty.FindPropertyRelative(proxyHelperPortData.GetFiledName()));
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
            ProxyHelperNodeData bindNodeData = (ProxyHelperNodeData)m_NodeData;
            IReadOnlyList<BaseNodeData> nodeData = bindNodeData.GetGraphAsset().GetNodesData();
            for (int i = 0; i < nodeData.Count; i++)
            {
                BaseNodeData baseNodeData = nodeData[i];
                if (baseNodeData != bindNodeData && baseNodeData is ProxyHelperNodeData proxyTargetNodeData)
                {
                    if (proxyTargetNodeData.IsInputPortsProxyHelper == evt.newValue)
                    {
                        m_IsProxyInputToggle.SetValueWithoutNotify(bindNodeData.IsInputPortsProxyHelper);
                        return;
                    }
                }
            }
            bindNodeData.IsInputPortsProxyHelper = evt.newValue;
            //TODO:移除所有端口，包括视图上的端口，连线；数据上的连线数据
        }
    }
}