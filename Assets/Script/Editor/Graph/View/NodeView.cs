using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using YBFramework.Bridge.Data;

namespace YBFramework.Editor.Graph
{
    public sealed class NodeView : Node
    {
        /// <summary>
        /// NodeView视图绑定的BaseNodeData。
        /// 正常MVP架构是不允许数据Data和视图View之间有联系，但是为了用户操作视图时，能够快捷的获取到数据才这么做，不然只有去Presenter中一级一级查找非常耗时。
        /// </summary>
        public readonly BaseNodeData BindNodeData;

        private readonly List<PortView> m_PortViews = new();

        public NodeView(BaseNodeData nodeData)
        {
            BindNodeData = nodeData;
            title = nodeData.NodeName;
            SetPosition(new Rect(nodeData.Position, Vector2.one));
        }

        public void AddPortContentView(VisualElement portContentView, Direction direction)
        {
            portContentView.style.borderBottomColor = Color.black;
            portContentView.style.borderBottomWidth = .2f;
            if (direction == Direction.Input)
            {
                inputContainer.Add(portContentView);
            }
            else
            {
                outputContainer.Add(portContentView);
            }
        }

        public void RemovePortContentView(VisualElement portContentView, Direction direction)
        {
            if (direction == Direction.Input)
            {
                inputContainer.Remove(portContentView);
            }
            else
            {
                outputContainer.Remove(portContentView);
            }
        }

        public IReadOnlyList<PortView> GetPortViews()
        {
            return m_PortViews;
        }

        public PortView GetPortView(int portID)
        {
            for (int i = 0; i < m_PortViews.Count; i++)
            {
                if (m_PortViews[i].BindPortData.GetPortID() == portID)
                {
                    return m_PortViews[i];
                }
            }
            return null;
        }

        //TODO:暂时处理
        public void AddPortView(PortView portView)
        {
            m_PortViews.Add(portView);
        }

        public void RemovePortView(PortView portView)
        {
            m_PortViews.Remove(portView);
        }

        public void ClearPortView()
        {
        }
        //

        public void RefreshPortContainerDisplay()
        {
            //TODO:对于输入输出只存在一个动态端口的情况时，初始动态端口没有任何值，但是在编辑完后会存在值，这时就需要显示这个端口（代理节点）
            inputContainer.style.display = inputContainer.childCount == 0 ? DisplayStyle.None : DisplayStyle.Flex;
            outputContainer.style.display = outputContainer.childCount == 0 ? DisplayStyle.None : DisplayStyle.Flex;
        }
    }
}