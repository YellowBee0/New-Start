using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using YBFramework.Bridge.Data;
using YBFramework.Editor.Graph.Presenter;

namespace YBFramework.Editor.Graph
{
    [RuntimeToEditor(typeof(SubNodeData))]
    public sealed class SubNodeDataPresenter : BaseNodeDataPresenter
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
            GraphAsset subGraphAsset = evt.newValue as GraphAsset;
            if (subGraphAsset != null && (subNodeData.GetGraphAsset().GraphType & subGraphAsset.GraphType) != subGraphAsset.GraphType)
            {
                Debug.LogError($"This graph type:{subNodeData.GetGraphAsset().GraphType} is not contains sub graph type:{subGraphAsset.GraphType}");
                m_SubGraphAssetField.SetValueWithoutNotify(subNodeData.GetSubGraphAsset());
                return;
            }
            //TODO:需要支持Undo
            //修改数据
            if (subNodeData.TrySetSubGraphAsset(subGraphAsset))
            {
                //修改视图
                subNodeData.InitializeSubPortsData();
                //通过GraphWindow获取打开的GraphView可能和当前修改的Port的GraphView不是同一个
                GraphPresenter graphPresenter = GraphWindow.GetInstance().GetOpenedPresenter();
                for (int i = 0; i < m_PortPresenters.Count; i++)
                {
                    BasePortDataPresenter portDataPresenter = m_PortPresenters[i];
                    CustomGraphView graphView = graphPresenter.GetGraphView();
                    CustomGraphView.DisconnectAll(graphView, portDataPresenter.GetPortView());
                    m_NodeView.RemovePortContentView(portDataPresenter.GetPortContentView(), portDataPresenter.GetPortData().GetDirection());
                    BasePortDataPresenter.ReleasePortPresenter(portDataPresenter);
                }
                m_PortPresenters.Clear();
                //创建端口时先更新数据
                graphPresenter.UpdateSO();
                //重新调用一次创建内部端口
                int portDataCount = m_NodeData.GetPortsDataCount();
                for (int i = 0; i < portDataCount; i++)
                {
                    BasePortData portData = m_NodeData.PortDataOfIndex(i);
                    BasePortDataPresenter portDataPresenter = BasePortDataPresenter.AllocatePortPresenter(portData.GetType());
                    if (portDataPresenter != null)
                    {
                        SerializedProperty nodeSerializedProperty = graphPresenter.GetNodeSerializedProperty(m_NodeData);
                        portDataPresenter.Initialize(portData, nodeSerializedProperty.FindPropertyRelative(portData.GetFieldName()));
                        AddPortPresenter(portDataPresenter);
                    }
                }
                m_NodeView.RefreshPortContainerDisplay();
                return;
            }
            m_SubGraphAssetField.SetValueWithoutNotify(subNodeData.GetSubGraphAsset());
        }
    }
}