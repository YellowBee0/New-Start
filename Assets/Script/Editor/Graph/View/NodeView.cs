using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using YBFramework.Bridge.Data;

namespace YBFramework.Editor
{
    public sealed class NodeView : Node
    {
        public readonly BaseNodeData BindNodeData;

        private readonly List<PortView> m_PortViews = new();
        
        private readonly BaseNodeDrawer m_NodeDrawer;

        public NodeView(BaseNodeData bindNodeData,BaseNodeDrawer nodeDrawer)
        {
            BindNodeData = bindNodeData;
            title = bindNodeData.Name;
            SetPosition(new Rect(bindNodeData.Position, Vector2.one));
        }

        public IReadOnlyList<PortView> GetPortViews()
        {
            return m_PortViews;
        }

        public PortView GetPortView(int portID)
        {
            for (int i = 0; i < m_PortViews.Count; i++)
            {
                if (m_PortViews[i].BindPortData.PortID == portID)
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