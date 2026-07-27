using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using YBFramework.Bridge.Data;
using YBFramework.Common;

namespace YBFramework.Editor.Graph
{
    [GraphDrawer(typeof(BaseNodeData))]
    public class BaseNodeDrawer
    {
        private static readonly Dictionary<Type, Stack<BaseNodeDrawer>> s_NodeDrawers = new();

        public static BaseNodeDrawer AllocateNodeDrawer(Type drawTargetType)
        {
            Type nodeDrawerType = GraphDrawerMap.GetInstance().GetDrawerType(drawTargetType);
            if (nodeDrawerType == null)
            {
                return null;
            }
            if (!s_NodeDrawers.TryGetValue(nodeDrawerType, out Stack<BaseNodeDrawer> nodeDrawers))
            {
                nodeDrawers = new Stack<BaseNodeDrawer>();
                s_NodeDrawers.Add(nodeDrawerType, nodeDrawers);
            }
            return nodeDrawers.Count > 0 ? nodeDrawers.Pop() : Activator.CreateInstance(nodeDrawerType) as BaseNodeDrawer;
        }

        public static void ReleaseNodeDrawer(BaseNodeDrawer nodeDrawer)
        {
            Type nodeDrawerType = nodeDrawer.GetType();
            if (s_NodeDrawers.TryGetValue(nodeDrawerType, out Stack<BaseNodeDrawer> nodeDrawers))
            {
                nodeDrawers.Push(nodeDrawer);
            }
        }

        protected static void CreatePortView(NodeView nodeView, SerializedProperty nodeSerializedProperty, BasePortData portData)
        {
            BasePortDrawer portDrawer = BasePortDrawer.AllocatePortDrawer(portData.GetType());
            if (portDrawer != null)
            {
                VisualElement portContentView = portDrawer.CreatePortContentView(portData, nodeSerializedProperty.FindPropertyRelative(portData.GetFiledName()), out PortView portView);
                portContentView.style.borderBottomColor = Color.black;
                portContentView.style.borderBottomWidth = .2f;
                if (portView.direction == Direction.Input)
                {
                    nodeView.inputContainer.Add(portContentView);
                }
                else
                {
                    nodeView.outputContainer.Add(portContentView);
                }
                nodeView.AddPortView(portView);
            }
        }

        protected BaseNodeData m_BindNodeData;

        public BaseNodeData GetBindNodeData()
        {
            return m_BindNodeData;
        }

        public virtual NodeView CreateNodeView(BaseNodeData nodeData, SerializedProperty serializedProperty)
        {
            /*m_BindNodeData = nodeData;
            NodeView nodeView = new(this)
            {
                title = nodeData.Name
            };
            nodeView.SetPosition(new Rect(nodeData.Position, Vector2.one));
            foreach (BasePortData portData in (IValueIterator<BasePortData>)nodeData)
            {
                CreatePortView(nodeView, serializedProperty, portData);
            }
            nodeView.RefreshPortContainerDisplay();
            return nodeView;*/
            return null;
        }
    }
}