using UnityEditor;
using UnityEditor.Experimental.GraphView;
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
            VisualElement portContentView = base.CreatePortContentView(portData, serializedProperty, out portView);
            SerializedProperty valueProperty = serializedProperty.FindPropertyRelative("Value");
            if (valueProperty != null)
            {
                VisualElement container = new();
                portView.RegisterOnConnectCallback(OnConnectOther);
                portView.RegisterOnDisconnectCallback(OnDisconnectOther);
                m_ValueField = new PropertyField();
                m_ValueField.styleSheets.Add(StyleSheetManager.LoadStylesheet("GraphViewLabel"));
                m_ValueField.BindProperty(valueProperty);
                container.Add(portContentView);
                container.Add(m_ValueField);
                return container;
            }
            return portContentView;
        }

        private void OnConnectOther(Port other)
        {
            m_ValueField.enabledSelf = false;
        }

        private void OnDisconnectOther(Port other)
        {
            m_ValueField.enabledSelf = true;
        }
    }
}