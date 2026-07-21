using System.Collections.Generic;
using UnityEditor;
using UnityEngine.UIElements;
using YBFramework.Bridge.Data;
using YBFramework.Bridge.Editor;

namespace YBFramework.Editor
{
    [GraphDrawer(typeof(ProxyTargetNodeData))]
    public sealed class ProxyTargetNodeDrawer : BaseNodeDrawer
    {
        private ProxyTargetNodeData m_DrawNodeData;

        private Toggle m_IsProxyInputToggle;

        public override NodeView CreateNodeView(BaseNodeData nodeData, SerializedProperty serializedProperty)
        {
            m_DrawNodeData = (ProxyTargetNodeData)nodeData;
            NodeView nodeView = base.CreateNodeView(nodeData, serializedProperty);
            m_IsProxyInputToggle = new Toggle
            {
                value = m_DrawNodeData.IsProxyInput
            };
            m_IsProxyInputToggle.RegisterValueChangedCallback(OnIsProxyInputChanged);
            nodeView.contentContainer.Add(m_IsProxyInputToggle);
            return nodeView;
        }

        private void OnIsProxyInputChanged(ChangeEvent<bool> evt)
        {
            IReadOnlyList<BaseNodeData> nodeData = m_DrawNodeData.GraphAsset.GetNodeData();
            for (int i = 0; i < nodeData.Count; i++)
            {
                BaseNodeData baseNodeData = nodeData[i];
                if (baseNodeData != m_DrawNodeData && baseNodeData is ProxyTargetNodeData proxyTargetNodeData)
                {
                    if (proxyTargetNodeData.IsProxyInput == evt.newValue)
                    {
                        m_IsProxyInputToggle.SetValueWithoutNotify(m_DrawNodeData.IsProxyInput);
                        return;
                    }
                }
            }
            m_DrawNodeData.IsProxyInput = evt.newValue;
            //TODO:移除所有端口，包括视图上的端口，连线；数据上的连线数据
        }
    }
}