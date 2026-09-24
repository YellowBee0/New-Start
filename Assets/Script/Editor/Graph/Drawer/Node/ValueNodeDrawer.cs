using UnityEditor;
using UnityEditor.UIElements;
using YBFramework.Bridge.Data;

namespace YBFramework.Editor.Graph
{
    [RuntimeToEditor(typeof(ValueNodeData<>))]
    public sealed class ValueNodeDrawer : BaseNodeDrawer
    {
        private readonly PropertyField m_ValueField;

        private bool m_HasAddValueField;

        public ValueNodeDrawer()
        {
            m_ValueField = new PropertyField();
            m_ValueField.styleSheets.Add(StyleSheetManager.LoadStylesheet("GraphViewLabel"));
        }

        protected override void OnDrawNodeView()
        {
            base.OnDrawNodeView();
            SerializedProperty serializedProperty = m_GraphAssetDrawer.GetNodeDataProperty(m_NodeData);
            if (serializedProperty != null)
            {
                SerializedProperty valueSerializedProperty = serializedProperty.FindPropertyRelative("m_Value");
                m_ValueField.BindProperty(valueSerializedProperty);
                m_NodeView.contentContainer.Add(m_ValueField);
                m_HasAddValueField = true;
            }
        }

        protected override void OnRelease()
        {
            base.OnRelease();
            if (m_HasAddValueField)
            {
                m_NodeView.contentContainer.Remove(m_ValueField);
            }
        }
    }
}