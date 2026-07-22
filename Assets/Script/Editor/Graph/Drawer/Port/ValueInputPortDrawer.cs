using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using YBFramework.Bridge.Data;

namespace YBFramework.Editor
{
    [GraphDrawer(typeof(ValueInputPortData<>))]
    public sealed class ValueInputPortDrawer : BasePortDrawer
    {
        private PropertyField m_ValueField;

        public override VisualElement CreatePortContentView(BasePortData portData, SerializedProperty serializedProperty, out PortView portView)
        {
            VisualElement portContentView = new();
            portContentView.Add(base.CreatePortContentView(portData, serializedProperty, out portView));
            //TODO:需要支持Undo，这里本身就能Undo但是不和我自己的Undo系统同步
            m_ValueField = new PropertyField(serializedProperty.FindPropertyRelative("Value"));
            portContentView.Add(m_ValueField);
            return portContentView;
        }
    }
}