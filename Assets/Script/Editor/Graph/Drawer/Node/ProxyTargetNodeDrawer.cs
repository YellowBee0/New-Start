using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using YBFramework.Bridge.Data;

namespace YBFramework.Editor
{
    [GraphDrawer(typeof(ProxyTargetNodeData))]
    public sealed class ProxyTargetNodeDrawer : BaseNodeDrawer
    {
        private Toggle m_IsProxyInputToggle;

        private SerializedProperty m_NodeSerializedProperty;

        private NodeView m_NodeView;

        public override NodeView CreateNodeView(BaseNodeData nodeData, SerializedProperty serializedProperty)
        {
            m_NodeSerializedProperty = serializedProperty;
            m_NodeView = base.CreateNodeView(nodeData, serializedProperty);
            VisualElement buttonContainer = new()
            {
                style =
                {
                    flexDirection = FlexDirection.Row
                }
            };
            Button addButton = new Button(OnAddClicked)
            {
                text = "添加"
            };
            Button removeButton = new Button(OnRemoveClicked)
            {
                text = "删除"
            };
            buttonContainer.Add(addButton);
            buttonContainer.Add(removeButton);
            ProxyTargetNodeData proxyTargetNodeData = (ProxyTargetNodeData)nodeData;
            m_IsProxyInputToggle = new Toggle("是否为输入端口集合")
            {
                value = proxyTargetNodeData.IsProxyInput
            };
            m_IsProxyInputToggle.RegisterValueChangedCallback(OnIsProxyInputChanged);
            m_NodeView.contentContainer.Add(m_IsProxyInputToggle);
            m_NodeView.contentContainer.Add(buttonContainer);
            return m_NodeView;
        }

        private void OnAddClicked()
        {
            ProxyTargetNodeData proxyTargetNodeData = (ProxyTargetNodeData)m_BindNodeData;
            ProxyTargetPortData proxyTargetPortData = new ProxyTargetPortData
            {
                NodeData = proxyTargetNodeData,
                PortID = ++proxyTargetNodeData.SourcePortID
            };
            proxyTargetPortData.CreateData();
            proxyTargetPortData.SetFiledName(string.Format(ProxyTargetNodeData.DEFAULT_FILED_NAME, proxyTargetNodeData.ProxyTargetPortsData.Count));
            proxyTargetPortData.SetPortName(string.Format(ProxyTargetNodeData.DEFAULT_PORT_NAME, proxyTargetNodeData.ProxyTargetPortsData.Count));
            Direction direction = proxyTargetNodeData.IsProxyInput ? Direction.Input : Direction.Output;
            proxyTargetPortData.SetDirection(direction);
            proxyTargetPortData.SetCapacity(ProxyTargetNodeData.DEFAULT_PORT_CAPACITY);
            proxyTargetPortData.SetPortColor(ProxyTargetNodeData.DefaultColor);
            proxyTargetPortData.SetCapacity(proxyTargetPortData.GetCapacity());
            proxyTargetNodeData.ProxyTargetPortsData.Add(proxyTargetPortData);
            m_NodeSerializedProperty.serializedObject.Update();
            CreatePortView(m_NodeView, m_NodeSerializedProperty, proxyTargetPortData);
            m_NodeView.RefreshPortContainerDisplay();
        }

        private void OnRemoveClicked()
        {
            Debug.Log("开发中");
        }

        private void OnIsProxyInputChanged(ChangeEvent<bool> evt)
        {
            ProxyTargetNodeData bindNodeData = (ProxyTargetNodeData)m_BindNodeData;
            IReadOnlyList<BaseNodeData> nodeData = bindNodeData.GraphAsset.GetNodeData();
            for (int i = 0; i < nodeData.Count; i++)
            {
                BaseNodeData baseNodeData = nodeData[i];
                if (baseNodeData != bindNodeData && baseNodeData is ProxyTargetNodeData proxyTargetNodeData)
                {
                    if (proxyTargetNodeData.IsProxyInput == evt.newValue)
                    {
                        m_IsProxyInputToggle.SetValueWithoutNotify(bindNodeData.IsProxyInput);
                        return;
                    }
                }
            }
            bindNodeData.IsProxyInput = evt.newValue;
            //TODO:移除所有端口，包括视图上的端口，连线；数据上的连线数据
        }
    }
}