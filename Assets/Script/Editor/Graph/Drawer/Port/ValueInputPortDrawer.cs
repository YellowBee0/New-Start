using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using YBFramework.Bridge.Data;

namespace YBFramework.Editor.Graph
{
    [RuntimeToEditor(typeof(ValueInputPortData<>))]
    public sealed class ValueInputPortDrawer : BasePortDrawer
    {
        private readonly PropertyField m_ValueField;

        private bool m_HasAddValueField;

        private BasePortData m_ValueInputPortData;

        private PortView m_PortView;

        private VisualElement m_PortContentView;

        public ValueInputPortDrawer()
        {
            m_ValueField = new PropertyField();
            m_ValueField.styleSheets.Add(StyleSheetManager.LoadStylesheet("GraphViewLabel"));
        }

        public override void OnPortViewConnect(Edge edge)
        {
            m_ValueField.enabledSelf = false;
        }

        public override void OnPortViewDisconnect(Edge edge)
        {
            m_ValueField.enabledSelf = true;
        }

        public override BasePortData GetPortData()
        {
            return m_ValueInputPortData;
        }

        public override VisualElement GetPortContentView()
        {
            return m_PortContentView;
        }

        public override PortView GetPortView()
        {
            return m_PortView;
        }

        protected override PortView OnDrawPortView(BasePortData portData)
        {
            m_ValueInputPortData = portData;
            m_PortView = PortView.Allocate(portData.GetDirection(), portData.GetCapacity(), portData.GetPortID(), this, portData.GetPortName(), portData.GetPortColor());
            m_PortContentView = m_PortView;
            if (portData is IFieldPath fieldPath)
            {
                SerializedProperty nodeDataSP = GetNodeDrawer().GetGraphAssetDrawer().GetNodeDataProperty(portData.GetNodeData());
                if (nodeDataSP != null)
                {
                    SerializedProperty portDataSP = nodeDataSP.FindPropertyRelative(fieldPath.GetFieldPath());
                    if (portDataSP != null)
                    {
                        SerializedProperty valueProperty = portDataSP.FindPropertyRelative("m_Value");
                        if (valueProperty != null)
                        {
                            VisualElement portContentView = new();
                            m_ValueField.BindProperty(valueProperty);
                            portContentView.Add(m_PortContentView);
                            portContentView.Add(m_ValueField);
                            m_PortContentView = portContentView;
                            m_HasAddValueField = true;
                        }
                    }
                }
            }
            return m_PortView;
        }

        public override void OnRelease()
        {
            if (m_HasAddValueField)
            {
                m_PortContentView.Remove(m_ValueField);
                m_HasAddValueField = false;
            }
        }
    }
}