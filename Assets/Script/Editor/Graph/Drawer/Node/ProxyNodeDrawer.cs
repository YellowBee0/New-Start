using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using YBFramework.Bridge.Data;
using YBFramework.Bridge.Editor;

namespace YBFramework.Editor
{
    public sealed class ProxyNodeDrawer : BaseNodeDrawer
    {
        private ProxyNodeData m_ProxyNodeData;

        private ObjectField m_ProxyGraphAssetField;
        
        public override NodeView CreateNodeView(BaseNodeData nodeData, SerializedProperty serializedProperty)
        {
            m_ProxyNodeData = (ProxyNodeData)nodeData;
            NodeView nodeView = base.CreateNodeView(nodeData, serializedProperty);
            m_ProxyGraphAssetField = new ObjectField
            {
                allowSceneObjects = false,
                value = m_ProxyNodeData.ProxyGraphAsset,
                objectType = typeof(GraphAsset)
            };
            m_ProxyGraphAssetField.RegisterValueChangedCallback(OnProxyGraphAssetChanged);
            nodeView.contentContainer.Add(m_ProxyGraphAssetField);
            return nodeView;
        }

        private void OnProxyGraphAssetChanged(ChangeEvent<Object> evt)
        {
            if (evt.newValue is GraphAsset proxyGraphAsset)
            {
                if ((m_ProxyNodeData.GetGraphAsset().GetGraphType() & proxyGraphAsset.GetGraphType()) == proxyGraphAsset.GetGraphType())
                {
                    //TODO:需要支持Undo
                    m_ProxyNodeData.ProxyGraphAsset = proxyGraphAsset;
                    return;
                }
                Debug.LogError($"This graph type:{m_ProxyNodeData.GetGraphAsset().GetGraphType()} is not contains proxy graph type:{proxyGraphAsset.GetGraphType()}");
            }
            else
            {
                Debug.LogError($"{evt.newValue} is not type of GraphAsset");
            }
            m_ProxyGraphAssetField.SetValueWithoutNotify(m_ProxyNodeData.ProxyGraphAsset);
        }
    }
}