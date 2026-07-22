using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

namespace YBFramework.Editor
{
    public sealed class NodeView : Node
    {
        public readonly BaseNodeDrawer BindNodeDrawer;

        private readonly List<PortView> m_PortViews = new();

        public NodeView(BaseNodeDrawer bindNodeDrawer)
        {
            BindNodeDrawer = bindNodeDrawer;
        }

        public IReadOnlyList<PortView> GetPortViews()
        {
            return m_PortViews;
        }

        public PortView GetPortView(int portID)
        {
            for (int i = 0; i < m_PortViews.Count; i++)
            {
                if (m_PortViews[i].BindPortDrawer.GetBindPortData().PortID == portID)
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

        public void OnRelease()
        {
            BaseNodeDrawer.ReleaseNodeDrawer(BindNodeDrawer);
            for (int i = 0; i < m_PortViews.Count; i++)
            {
                m_PortViews[i].OnRelease();
            }
        }
    }
}