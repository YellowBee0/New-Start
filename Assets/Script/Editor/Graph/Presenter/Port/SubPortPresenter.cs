using UnityEditor;
using UnityEngine.UIElements;
using YBFramework.Bridge.Data;

namespace YBFramework.Editor.Graph
{
    [RuntimeToEditor(typeof(SubPortData))]
    public sealed class SubPortPresenter : BasePortPresenter
    {
        private BasePortPresenter m_InternalPortPresenter;

        private SubPortData m_PortData;

        public override void Initialize(BasePortData portData, SerializedProperty portSerializedProperty)
        {
            m_PortData = (SubPortData)portData;
            BasePortData subPortData = m_PortData.GetAsSubPortData();
            m_InternalPortPresenter = AllocatePortPresenter(subPortData.GetType());
            if (m_InternalPortPresenter != null)
            {
                m_InternalPortPresenter.Initialize(subPortData, portSerializedProperty.FindPropertyRelative(subPortData.GetFieldName()));
            }
        }

        public override BasePortData GetPortData()
        {
            return m_PortData;
        }

        public override PortView GetPortView()
        {
            return m_InternalPortPresenter.GetPortView();
        }

        public override VisualElement GetPortContentView()
        {
            return m_InternalPortPresenter.GetPortContentView();
        }
    }
}