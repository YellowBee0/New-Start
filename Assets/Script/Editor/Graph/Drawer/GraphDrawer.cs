using System.Collections.Generic;
using UnityEditor;
using YBFramework.Bridge.Data;

namespace YBFramework.Editor
{
    public sealed class GraphDrawer
    {
        private GraphAsset m_GraphAsset;

        private SerializedObject m_SerializedObject;

        private SerializedProperty m_NodeDataListProperty;

        public CustomGraphView CreateGraphView(GraphAsset graphAsset)
        {
            m_GraphAsset = graphAsset;
            m_SerializedObject = new SerializedObject(graphAsset);
            m_NodeDataListProperty = m_SerializedObject.FindProperty("m_NodeData");
            CustomGraphView graphView = new(graphAsset, NodeSearchEntry.GetSearchEntry(graphAsset.GetGraphType()), this);
            IReadOnlyList<BaseNodeData> nodeData = graphAsset.GetNodeData();
            for (int i = 0; i < nodeData.Count; i++)
            {
                BaseNodeData baseNodeData = nodeData[i];
                BaseNodeDrawer nodeDrawer = GraphDrawerMap.GetInstance().GetNodeDrawer(baseNodeData.GetType());
                if (nodeDrawer != null)
                {
                    NodeView nodeView = nodeDrawer.CreateNodeView(baseNodeData, m_NodeDataListProperty.GetArrayElementAtIndex(i));
                    graphView.AddNodeView(nodeView);
                }
            }
            return graphView;
        }
    }
}