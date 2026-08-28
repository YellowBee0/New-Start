using UnityEditor;
using UnityEngine.UIElements;
using YBFramework.Bridge.Data;

namespace YBFramework.Editor.Graph
{
    [RuntimeToEditor(typeof(SubPortData))]
    public sealed class SubPortDataDataPresenter : BasePortDataPresenter
    {
        private BasePortDataPresenter m_AsSubPortDataDataPresenter;

        public override void Initialize(BaseNodeDataPresenter nodeDataPresenter, BasePortData portData, SerializedProperty portSerializedProperty)
        {
            base.Initialize(nodeDataPresenter, portData, portSerializedProperty);
            SubPortData subPortData = (SubPortData)portData;
            BasePortData asSubPortData = subPortData.GetAsSubPortData();
            m_AsSubPortDataDataPresenter = AllocatePortPresenter(asSubPortData.GetType());
            if (m_AsSubPortDataDataPresenter != null)
            {
                m_AsSubPortDataDataPresenter.Initialize(nodeDataPresenter, asSubPortData, portSerializedProperty.FindPropertyRelative(asSubPortData.GetFieldName()));
            }
        }

        public override BasePortData GetPortData()
        {
            return m_AsSubPortDataDataPresenter.GetPortData();
        }

        public override PortView GetPortView()
        {
            return m_AsSubPortDataDataPresenter.GetPortView();
        }

        public override VisualElement GetPortContentView()
        {
            return m_AsSubPortDataDataPresenter.GetPortContentView();
        }
    }
}