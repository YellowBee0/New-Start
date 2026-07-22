using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using YBFramework.Bridge.Data;

namespace YBFramework.Editor
{
    [GraphDrawer(typeof(ProxyNodeData))]
    public sealed class ProxyNodeDrawer : BaseNodeDrawer
    {
        private ObjectField m_ProxyGraphAssetField;

        public override NodeView CreateNodeView(BaseNodeData nodeData, SerializedProperty serializedProperty)
        {
            NodeView nodeView = base.CreateNodeView(nodeData, serializedProperty);
            ProxyNodeData proxyNodeData = (ProxyNodeData)nodeData;
            m_ProxyGraphAssetField = new ObjectField
            {
                allowSceneObjects = false,
                value = proxyNodeData.ProxyGraphAsset,
                objectType = typeof(GraphAsset)
            };
            m_ProxyGraphAssetField.RegisterValueChangedCallback(OnProxyGraphAssetChanged);
            nodeView.contentContainer.Add(m_ProxyGraphAssetField);
            return nodeView;
        }

        private void OnProxyGraphAssetChanged(ChangeEvent<Object> evt)
        {
            ProxyNodeData proxyNodeData = (ProxyNodeData)m_BindNodeData;
            if (evt.newValue is GraphAsset proxyGraphAsset)
            {
                if ((proxyNodeData.GraphAsset.GetGraphType() & proxyGraphAsset.GetGraphType()) == proxyGraphAsset.GetGraphType())
                {
                    //TODO:需要支持Undo
                    proxyNodeData.ProxyGraphAsset = proxyGraphAsset;
                    return;
                }
                Debug.LogError($"This graph type:{proxyNodeData.GraphAsset.GetGraphType()} is not contains proxy graph type:{proxyGraphAsset.GetGraphType()}");
            }
            else
            {
                Debug.LogError($"{evt.newValue} is not type of GraphAsset");
            }
            m_ProxyGraphAssetField.SetValueWithoutNotify(proxyNodeData.ProxyGraphAsset);
        }
    }
}