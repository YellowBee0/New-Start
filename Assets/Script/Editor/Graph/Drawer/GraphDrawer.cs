using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using YBFramework.Bridge.Data;

namespace YBFramework.Editor.Graph
{
    public sealed class GraphDrawer
    {
        private GraphAsset m_BindGraphAsset;

        private NodeSearchEntry m_BindNodeSearchEntry;

        /// <summary>
        /// 该字段仅用于缓存CustomGraphView给graphViewChanged这个事件使用（为了避免闭包）。后续感觉可以直接使用GraphWindow的MainGraphView
        /// </summary>
        private CustomGraphView m_GraphView;

        private SerializedObject m_SerializedObject;

        private SerializedProperty m_NodeDataListProperty;

        public CustomGraphView CreateGraphView(GraphAsset graphAsset, SerializedObject serializedObject)
        {
            m_BindGraphAsset = graphAsset;
            //m_GraphView = new CustomGraphView(this);
            m_BindNodeSearchEntry = NodeSearchEntry.GetSearchEntry(graphAsset.GetGraphType());
            m_SerializedObject = serializedObject;
            m_NodeDataListProperty = m_SerializedObject.FindProperty("m_NodeData");

            m_GraphView.nodeCreationRequest = ShowNodeSearchView;
            m_GraphView.graphViewChanged += OnGraphViewChanged;

            IReadOnlyList<BaseNodeData> nodeData = graphAsset.GetNodesData();
            for (int i = 0; i < nodeData.Count; i++)
            {
                BaseNodeData baseNodeData = nodeData[i];
                BaseNodeDrawer nodeDrawer = BaseNodeDrawer.AllocateNodeDrawer(baseNodeData.GetType());
                if (nodeDrawer != null)
                {
                    m_GraphView.AddNodeView(nodeDrawer.CreateNodeView(baseNodeData, m_NodeDataListProperty.GetArrayElementAtIndex(i)));
                }
            }
            return m_GraphView;
        }

        public GraphAsset GetBindGraphAsset()
        {
            return m_BindGraphAsset;
        }

        public void UpdateSO()
        {
            m_SerializedObject.Update();
        }

        public SerializedProperty GetNodeSerializedProperty(BaseNodeData nodeData)
        {
            IReadOnlyList<BaseNodeData> existNodeData = m_BindGraphAsset.GetNodesData();
            for (int i = 0; i < existNodeData.Count; i++)
            {
                if (existNodeData[i] == nodeData)
                {
                    return m_NodeDataListProperty.GetArrayElementAtIndex(i);
                }
            }
            return null;
        }

        private GraphViewChange OnGraphViewChanged(GraphViewChange changeValue)
        {
            //TODO:需要支持Undo
            /*if (changeValue.elementsToRemove != null)
            {
                for (int i = 0; i < changeValue.elementsToRemove.Count; i++)
                {
                    if (changeValue.elementsToRemove[i] is NodeView nodeView)
                    {
                        m_BindGraphAsset.RemoveNodeData(nodeView.BindNodeDrawer.GetBindNodeData());
                        m_GraphView.RemoveNodeView(nodeView);
                    }
                    else if (changeValue.elementsToRemove[i] is Edge edge)
                    {
                        PortView inputPortView = (PortView)edge.input;
                        PortView outputPortView = (PortView)edge.output;
                        inputPortView.BindPortDrawer.GetBindPortData().Disconnect(outputPortView.BindPortDrawer.GetBindPortData());
                        outputPortView.BindPortDrawer.GetBindPortData().Disconnect(inputPortView.BindPortDrawer.GetBindPortData());
                        edge.input.Disconnect(edge);
                        edge.output.Disconnect(edge);
                        m_GraphView.RemoveElement(edge);
                    }
                }
            }
            if (changeValue.movedElements != null)
            {
                for (int i = 0; i < changeValue.movedElements.Count; i++)
                {
                    if (changeValue.movedElements[i] is NodeView nodeView)
                    {
                        nodeView.BindNodeDrawer.GetBindNodeData().Position += changeValue.moveDelta;
                    }
                }
            }*/
            return changeValue;
        }

        private void ShowNodeSearchView(NodeCreationContext context)
        {
            SearchWindow.Open(new SearchWindowContext(context.screenMousePosition), m_BindNodeSearchEntry);
        }
    }
}