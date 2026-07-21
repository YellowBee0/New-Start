using System;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using YBFramework.Bridge.Data;
using YBFramework.Bridge.Editor;

namespace YBFramework.Editor
{
    [GraphDrawer(typeof(BaseNodeData))]
    public class BaseNodeDrawer
    {
        public virtual NodeView CreateNodeView(BaseNodeData nodeData, SerializedProperty serializedProperty)
        {
            NodeView nodeView = new(nodeData);
            //不使用原本的SerializedProperty进行轮询的原因：SerializedProperty轮询后不能回到起始位置，只能在最后一个SerializedProperty位置，这会导致用不到根SerializedProperty
            SerializedProperty serializedPropertyCopy = serializedProperty.Copy();
            while (serializedPropertyCopy.NextVisible(false))
            {
                //为什么强转后还要再去nodeData中找一次portData？因为boxedValue是另外一个实例，里面只有可以序列化的数据，其他数据都变成了默认值，所以必须回去找到真实的对象才能保证绘制
                if (serializedPropertyCopy.boxedValue is BasePortData portData)
                {
                    BasePortData actualPortData = nodeData.GetPortData(portData.PortID);
                    //TODO:获取端口绘制器
                    Type drawerType = GraphDrawerMap.GetInstance().GetDrawerType(actualPortData.GetType());
                    BasePortDrawer portDrawer = Activator.CreateInstance(drawerType) as BasePortDrawer;
                    VisualElement visualElement = portDrawer!.CreatePortContentView(actualPortData, serializedPropertyCopy, out PortView portView);
                    PortViewArgs portViewArgs = portData.GetPortViewArgs();
                    if (portViewArgs.Direction == Direction.Input)
                    {
                        nodeView.inputContainer.Add(visualElement);
                    }
                    else
                    {
                        nodeView.outputContainer.Add(visualElement);
                    }
                    nodeView.Add(portView);
                }
            }
            nodeView.RefreshPortContainerDisplay();
            return nodeView;
        }
    }
}