using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using YBFramework.Bridge.Data;

namespace YBFramework.Editor
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

        protected BaseNodeData m_BindNodeData;

        public BaseNodeData GetBindNodeData()
        {
            return m_BindNodeData;
        }

        public virtual NodeView CreateNodeView(BaseNodeData nodeData, SerializedProperty serializedProperty)
        {
            m_BindNodeData = nodeData;
            NodeView nodeView = new(this)
            {
                title = nodeData.Name
            };
            nodeView.SetPosition(new Rect(nodeData.Position, Vector2.one));
            //不使用原本的SerializedProperty进行轮询的原因：SerializedProperty轮询后不能回到起始位置，只能在最后一个SerializedProperty位置，这会导致用不到根SerializedProperty
            SerializedProperty serializedPropertyCopy = serializedProperty.Copy();
            while (serializedPropertyCopy.NextVisible(false))
            {
                //为什么强转后还要再去nodeData中找一次portData？因为boxedValue是另外一个实例，里面只有可以序列化的数据，其他数据都变成了默认值，所以必须回去找到真实的对象才能保证绘制
                if (serializedPropertyCopy.boxedValue is BasePortData portData)
                {
                    BasePortData actualPortData = nodeData.GetPortData(portData.PortID);
                    BasePortDrawer portDrawer = BasePortDrawer.AllocatePortDrawer(actualPortData.GetType());
                    if (portDrawer != null)
                    {
                        VisualElement portContentView = portDrawer.CreatePortContentView(actualPortData, serializedPropertyCopy, out PortView portView);
                        if (portView.direction == Direction.Input)
                        {
                            nodeView.inputContainer.Add(portContentView);
                        }
                        else
                        {
                            nodeView.outputContainer.Add(portContentView);
                        }
                        nodeView.Add(portView);
                    }
                }
            }
            nodeView.RefreshPortContainerDisplay();
            return nodeView;
        }
    }
}