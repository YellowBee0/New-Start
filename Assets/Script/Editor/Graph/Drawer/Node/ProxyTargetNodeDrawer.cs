using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using YBFramework.Bridge.Data;

namespace YBFramework.Editor
{
    [GraphDrawer(typeof(ProxyHelperNodeData))]
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
            ProxyHelperNodeData proxyHelperNodeData = (ProxyHelperNodeData)nodeData;
            m_IsProxyInputToggle = new Toggle("是否为输入端口代理辅助")
            {
                value = proxyHelperNodeData.IsInputPortsProxyHelper
            };
            m_IsProxyInputToggle.RegisterValueChangedCallback(OnIsProxyInputChanged);
            m_NodeView.contentContainer.Add(m_IsProxyInputToggle);
            m_NodeView.contentContainer.Add(buttonContainer);
            return m_NodeView;
        }

        private void OnAddClicked()
        {
            ProxyHelperNodeData proxyHelperNodeData = (ProxyHelperNodeData)m_BindNodeData;
            ProxyHelperPortData proxyHelperPortData = new ProxyHelperPortData
            {
                PortID = ++proxyHelperNodeData.SourcePortID
            };
            proxyHelperPortData.SetNodeData(proxyHelperNodeData);
            proxyHelperPortData.CreateData();
            proxyHelperPortData.SetFiledName(string.Format(ProxyHelperNodeData.PORT_HELPER_DATA_PATH, proxyHelperNodeData.ProxyHelperPortsData.Count));
            proxyHelperPortData.SetPortName(string.Format(ProxyHelperNodeData.PORT_HELPER_NAME, proxyHelperNodeData.ProxyHelperPortsData.Count));
            Direction direction = proxyHelperNodeData.IsInputPortsProxyHelper ? Direction.Input : Direction.Output;
            proxyHelperPortData.SetDirection(direction);
            proxyHelperPortData.SetCapacity(ProxyHelperNodeData.DEFAULT_PORT_CAPACITY);
            proxyHelperPortData.SetPortColor(ProxyHelperNodeData.DefaultColor);
            proxyHelperPortData.SetCapacity(proxyHelperPortData.GetCapacity());
            proxyHelperNodeData.ProxyHelperPortsData.Add(proxyHelperPortData);
            m_NodeSerializedProperty.serializedObject.Update();
            CreatePortView(m_NodeView, m_NodeSerializedProperty, proxyHelperPortData);
            m_NodeView.RefreshPortContainerDisplay();
        }

        private void OnRemoveClicked()
        {
            Debug.Log("开发中");
        }

        private void OnIsProxyInputChanged(ChangeEvent<bool> evt)
        {
            ProxyHelperNodeData bindNodeData = (ProxyHelperNodeData)m_BindNodeData;
            IReadOnlyList<BaseNodeData> nodeData = bindNodeData.GetGraphAsset().GetNodeData();
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