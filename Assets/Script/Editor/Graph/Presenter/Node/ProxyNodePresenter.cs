using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using YBFramework.Bridge.Data;
using YBFramework.Common;
using YBFramework.Editor.Graph.Presenter;

namespace YBFramework.Editor.Graph
{
    [RuntimeToEditor(typeof(ProxyNodeData))]
    public sealed class ProxyNodePresenter : BaseNodePresenter
    {
        private ObjectField m_ProxyGraphAssetField;

        public override void Initialize(BaseNodeData nodeData, SerializedProperty nodeSerializedProperty)
        {
            base.Initialize(nodeData, nodeSerializedProperty);
            ProxyNodeData proxyNodeData = (ProxyNodeData)nodeData;
            m_ProxyGraphAssetField = new ObjectField
            {
                allowSceneObjects = false,
                value = proxyNodeData.ProxyGraphAsset,
                objectType = typeof(GraphAsset)
            };
            m_ProxyGraphAssetField.RegisterValueChangedCallback(OnProxyGraphAssetChanged);
            m_NodeView.contentContainer.Add(m_ProxyGraphAssetField);
        }

        private void OnProxyGraphAssetChanged(ChangeEvent<Object> evt)
        {
            ProxyNodeData proxyNodeData = (ProxyNodeData)m_NodeData;
            if (evt.newValue is GraphAsset proxyGraphAsset)
            {
                if ((proxyNodeData.GetGraphAsset().GetGraphType() & proxyGraphAsset.GetGraphType()) == proxyGraphAsset.GetGraphType())
                {
                    //TODO:需要支持Undo
                    //proxyNodeData.ProxyGraphAsset = proxyGraphAsset;
                    //通过GraphWindow获取打开的GraphView可能和当前修改的Port的GraphView不是同一个
                    GraphPresenter graphPresenter = GraphWindow.GetInstance().GetOpenedPresenter();
                    for (int i = 0; i < m_PortPresenters.Count; i++)
                    {
                        BasePortPresenter portPresenter = m_PortPresenters[i];
                        portPresenter.GetPortData().DisconnectAll();
                        //通过GraphWindow获取打开的GraphView可能和当前修改的Port的GraphView不是同一个
                        CustomGraphView graphView = graphPresenter.GetGraphView();
                        CustomGraphView.DisconnectAll(graphView, portPresenter.GetPortView());
                        m_NodeView.RemovePortContentView(portPresenter.GetPortContentView(),portPresenter.GetPortData().GetDirection());
                        portPresenter.OnRelease();
                    }
                    m_PortPresenters.Clear();
                    proxyNodeData.ChangeProxyGraphAsset(proxyGraphAsset);
                    //创建端口时先更新数据
                    graphPresenter.UpdateSO();
                    
                    //重新调用一次创建内部端口
                    foreach (BasePortData portData in (IValueIterator<BasePortData>)m_NodeData)
                    {
                        BasePortPresenter portPresenter = BasePortPresenter.AllocatePortPresenter(portData.GetType());
                        if (portPresenter != null)
                        {
                            SerializedProperty nodeSerializedProperty = graphPresenter.GetNodeSerializedProperty(m_NodeData);
                            portPresenter.Initialize(portData, nodeSerializedProperty.FindPropertyRelative(portData.GetFiledName()));
                            AddPortPresenter(portPresenter);
                        }
                    }
                    m_NodeView.RefreshPortContainerDisplay();
                    return;
                }
                Debug.LogError($"This graph type:{proxyNodeData.GetGraphAsset().GetGraphType()} is not contains proxy graph type:{proxyGraphAsset.GetGraphType()}");
            }
            else
            {
                Debug.LogError($"{evt.newValue} is not type of GraphAsset");
            }
            m_ProxyGraphAssetField.SetValueWithoutNotify(proxyNodeData.ProxyGraphAsset);
        }
    }
}