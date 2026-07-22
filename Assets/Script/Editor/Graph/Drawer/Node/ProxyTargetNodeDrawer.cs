using System.Collections.Generic;
using UnityEditor;
using UnityEngine.UIElements;
using YBFramework.Bridge.Data;

namespace YBFramework.Editor
{
    [GraphDrawer(typeof(ProxyTargetNodeData))]
    public sealed class ProxyTargetNodeDrawer : BaseNodeDrawer
    {
        private Toggle m_IsProxyInputToggle;

        public override NodeView CreateNodeView(BaseNodeData nodeData, SerializedProperty serializedProperty)
        {
            NodeView nodeView = base.CreateNodeView(nodeData, serializedProperty);
            ProxyTargetNodeData proxyTargetNodeData = (ProxyTargetNodeData)nodeData;
            m_IsProxyInputToggle = new Toggle
            {
                value = proxyTargetNodeData.IsProxyInput
            };
            m_IsProxyInputToggle.RegisterValueChangedCallback(OnIsProxyInputChanged);
            nodeView.contentContainer.Add(m_IsProxyInputToggle);
            return nodeView;
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