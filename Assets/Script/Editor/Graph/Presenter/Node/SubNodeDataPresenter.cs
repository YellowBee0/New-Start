using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using YBFramework.Bridge.Data;

namespace YBFramework.Editor.Graph
{
    [RuntimeToEditor(typeof(SubNodeData))]
    public sealed class SubNodeDataPresenter : BaseNodeDataPresenter
    {
        private ObjectField m_SubGraphAssetField;

        public override void Initialize(GraphAssetPresenter graphAssetPresenter, BaseNodeData nodeData, SerializedProperty nodeSerializedProperty)
        {
            base.Initialize(graphAssetPresenter, nodeData, nodeSerializedProperty);
            SubNodeData subNodeData = (SubNodeData)nodeData;
            m_SubGraphAssetField = new ObjectField
            {
                allowSceneObjects = false,
                value = subNodeData.GetSubGraphAsset(),
                objectType = typeof(GraphAsset)
            };
            m_SubGraphAssetField.RegisterValueChangedCallback(OnSubGraphAssetChanged);
            m_NodeView.contentContainer.Add(m_SubGraphAssetField);
        }

        private void OnSubGraphAssetChanged(ChangeEvent<Object> evt)
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
                //初始化视图数据
                subNodeData.InitializeSubPortsData();
                //清空原有端口视图
                m_NodeView.ClearPortContentViews();
                //清空原有Presenter并释放
                for (int i = 0; i < m_PortPresenters.Count; i++)
                {
                    BasePortDataPresenter.ReleasePortPresenter(m_PortPresenters[i]);
                }
                m_PortPresenters.Clear();
                //创建端口时先更新数据
                m_GraphAssetPresenter.UpdateSO();
                //重新调用一次创建内部端口
                int portDataCount = m_NodeData.GetPortsDataCount();
                for (int i = 0; i < portDataCount; i++)
                {
                    BasePortData portData = m_NodeData.PortDataOfIndex(i);
                    BasePortDataPresenter portDataPresenter = BasePortDataPresenter.AllocatePortPresenter(portData.GetType());
                    if (portDataPresenter != null)
                    {
                        SerializedProperty nodeSerializedProperty = m_GraphAssetPresenter.GetNodeSerializedProperty(m_NodeData);
                        portDataPresenter.Initialize(this, portData, nodeSerializedProperty.FindPropertyRelative(portData.GetFieldName()));
                        AddPortDataPresenter(portDataPresenter);
                    }
                }
                m_NodeView.RefreshPortContainerDisplay();
                return;
            }
            m_SubGraphAssetField.SetValueWithoutNotify(subNodeData.GetSubGraphAsset());
        }
    }
}