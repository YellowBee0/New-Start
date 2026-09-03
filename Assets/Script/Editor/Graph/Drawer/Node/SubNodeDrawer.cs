using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using YBFramework.Bridge.Data;

namespace YBFramework.Editor.Graph
{
    [RuntimeToEditor(typeof(SubNodeData))]
    public sealed class SubNodeDrawer : BaseNodeDrawer
    {
        private ObjectField m_SubGraphAssetField;

        public SubNodeDrawer()
        {
            m_SubGraphAssetField = new ObjectField
            {
                allowSceneObjects = false,
                objectType = typeof(GraphAsset)
            };
            m_SubGraphAssetField.RegisterValueChangedCallback(OnSubGraphAssetChanged);
        }

        protected override NodeView OnDrawNodeView(BaseNodeData nodeData)
        {
            base.OnDrawNodeView(nodeData);
            SubNodeData subNodeData = (SubNodeData)nodeData;
            m_SubGraphAssetField.SetValueWithoutNotify(subNodeData.GetSubGraphAsset());
            m_NodeView.contentContainer.Add(m_SubGraphAssetField);
            return m_NodeView;
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
                //清空所有port data presenter
                //TODO:暂时处理
                OnRelease();
                //
                //创建端口时先更新数据
                m_GraphAssetDrawer.GetSO().Update();
                //TODO:实现节点下所有端口的删除和重建，可应用在这里和ExposePortsNodeDrawer的切换端口方向事件中，同时Undo也需要这个功能
                //重新调用一次创建内部端口
                int portDataCount = m_NodeData.GetPortsDataCount();
                for (int i = 0; i < portDataCount; i++)
                {
                    BasePortData portData = m_NodeData.PortDataOfIndex(i);
                    BasePortDrawer portDrawer = BasePortDrawer.Allocate(portData.GetType());
                    if (portDrawer != null)
                    {
                        m_NodeView.AddPortView(portDrawer.DrawPortView(this, portData));
                        AddPortDrawer(portDrawer);
                    }
                }
                m_NodeView.RefreshPortContainerDisplay();
                return;
            }
            m_SubGraphAssetField.SetValueWithoutNotify(subNodeData.GetSubGraphAsset());
        }
    }
}