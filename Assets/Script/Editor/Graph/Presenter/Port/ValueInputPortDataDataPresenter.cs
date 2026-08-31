using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using YBFramework.Bridge.Data;

namespace YBFramework.Editor.Graph
{
    [RuntimeToEditor(typeof(ValueInputPortData<>))]
    public sealed class ValueInputPortDataDataPresenter : BasePortDataPresenter
    {
        private PropertyField m_ValueField;

        private BasePortData m_PortData;

        private PortView m_PortView;

        private VisualElement m_PortContentView;

        public override void Initialize(BaseNodeDataPresenter nodeDataPresenter, BasePortData portData, SerializedProperty portSerializedProperty)
        {
            base.Initialize(nodeDataPresenter, portData, portSerializedProperty);
            m_PortData = portData;
            m_PortView = new PortView(this, portData.GetPortName(), portData.GetDirection(), portData.GetCapacity(), portData.GetPortColor());
            m_PortContentView = m_PortView;
            SerializedProperty valueProperty = portSerializedProperty.FindPropertyRelative("m_Value");
            if (valueProperty != null)
            {
                VisualElement portContentView = new();
                m_ValueField = new PropertyField();
                m_ValueField.styleSheets.Add(StyleSheetManager.LoadStylesheet("GraphViewLabel"));
                m_ValueField.BindProperty(valueProperty);
                portContentView.Add(m_PortContentView);
                portContentView.Add(m_ValueField);
                m_PortContentView = portContentView;
            }
        }

        public override BasePortData GetPortData()
        {
            return m_PortData;
        }

        public override PortView GetPortView()
        {
            return m_PortView;
        }

        public override VisualElement GetPortContentView()
        {
            return m_PortContentView;
        }

        public override void OnPortViewConnect(Edge edge)
        {
            m_ValueField.enabledSelf = false;
        }

        public override void OnPortViewDisconnect(Edge edge)
        {
            m_ValueField.enabledSelf = true;
        }
    }
}