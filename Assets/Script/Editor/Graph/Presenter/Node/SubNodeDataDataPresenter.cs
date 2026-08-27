using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using YBFramework.Bridge.Data;
using YBFramework.Editor.Graph.Presenter;

namespace YBFramework.Editor.Graph
{
    [RuntimeToEditor(typeof(SubNodeData))]
    public sealed class SubNodeDataDataPresenter : BaseNodeDataPresenter
    {
        private ObjectField m_SubGraphAssetField;

        public override void Initialize(BaseNodeData nodeData, SerializedProperty nodeSerializedProperty)
        {
            base.Initialize(nodeData, nodeSerializedProperty);
            SubNodeData subNodeData = (SubNodeData)nodeData;
            m_SubGraphAssetField = new ObjectField
            {
                allowSceneObjects = false,
                value = subNodeData.GetSubGraphAsset(),
                objectType = typeof(GraphAsset)
            };
            m_SubGraphAssetField.RegisterValueChangedCallback(OnProxyGraphAssetChanged);
            m_NodeView.contentContainer.Add(m_SubGraphAssetField);
        }

        private void OnProxyGraphAssetChanged(ChangeEvent<Object> evt)
        {
            SubNodeData subNodeData = (SubNodeData)m_NodeData;
            if (evt.newValue is GraphAsset subGraphAsset)
            {
                if ((subNodeData.GetGraphAsset().GraphType & subGraphAsset.GraphType) == subGraphAsset.GraphType)
                {
                    //TODO:需要支持Undo
                    //修改数据
                    subNodeData.SetSubGraphAsset(subGraphAsset);
                    //修改视图
                    subNodeData.InitializeSubPortsData();
                    //通过GraphWindow获取打开的GraphView可能和当前修改的Port的GraphView不是同一个
                    GraphPresenter graphPresenter = GraphWindow.GetInstance().GetOpenedPresenter();
                    for (int i = 0; i < m_PortPresenters.Count; i++)
                    {
                        BasePortPresenter portPresenter = m_PortPresenters[i];
                        CustomGraphView graphView = graphPresenter.GetGraphView();
                        CustomGraphView.DisconnectAll(graphView, portPresenter.GetPortView());
                        m_NodeView.RemovePortContentView(portPresenter.GetPortContentView(), portPresenter.GetPortData().GetDirection());
                        BasePortPresenter.ReleasePortPresenter(portPresenter);
                    }
                    m_PortPresenters.Clear();
                    //创建端口时先更新数据
                    graphPresenter.UpdateSO();
                    //重新调用一次创建内部端口
                    int portDataCount = m_NodeData.GetPortsDataCount();
                    for (int i = 0; i < portDataCount; i++)
                    {
                        BasePortData portData = m_NodeData.PortDataOfIndex(i);
                        BasePortPresenter portPresenter = BasePortPresenter.AllocatePortPresenter(portData.GetType());
                        if (portPresenter != null)
                        {
                            SerializedProperty nodeSerializedProperty = graphPresenter.GetNodeSerializedProperty(m_NodeData);
                            portPresenter.Initialize(portData, nodeSerializedProperty.FindPropertyRelative(portData.GetFieldName()));
                            AddPortPresenter(portPresenter);
                        }
                    }
                    m_NodeView.RefreshPortContainerDisplay();
                    return;
                }
                Debug.LogError($"This graph type:{subNodeData.GetGraphAsset().GraphType} is not contains sub graph type:{subGraphAsset.GraphType}");
            }
            else
            {
                Debug.LogError($"{evt.newValue} is not type of GraphAsset");
            }
            m_SubGraphAssetField.SetValueWithoutNotify(subNodeData.GetSubGraphAsset());
        }
    }
}