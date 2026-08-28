using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace YBFramework.Editor.Graph
{
    public sealed class NodeView : Node
    {
        private static readonly List<Edge> s_RemoveEdgesCache = new();

        public readonly BaseNodeDataPresenter NodeDataPresenter;

        private readonly List<PortView> m_PortViews = new();

        public NodeView(BaseNodeDataPresenter nodeDataPresenter)
        {
            NodeDataPresenter = nodeDataPresenter;
            title = nodeDataPresenter.GetNodeData().NodeName;
            SetPosition(new Rect(nodeDataPresenter.GetNodeData().Position, Vector2.one));
        }

        /// <summary>
        /// 在节点视图中添加一个端口视图，portContentView视图包含portView视图。
        /// 这里分开写是为了避免想要获取PortView还需要去portContentView查找一次。
        /// </summary>
        /// <param name="portContentView">端口完整视图</param>
        /// <param name="portView">单个portView视图，仅用于GraphView的GetCompatiblePorts函数使用（获取可连接端口）</param>
        public void AddPortContentView(VisualElement portContentView, PortView portView)
        {
            portContentView.style.borderBottomColor = Color.black;
            portContentView.style.borderBottomWidth = .2f;
            if (portView.direction == Direction.Input)
            {
                inputContainer.Add(portContentView);
            }
            else
            {
                outputContainer.Add(portContentView);
            }
            m_PortViews.Add(portView);
        }

        /// <summary>
        /// 在节点视图中移除一个端口视图，portContentView视图包含portView视图。
        /// 这里分开写是为了避免想要获取PortView还需要去portContentView查找一次。
        /// </summary>
        /// <param name="portContentView">端口完整视图</param>
        /// <param name="portView">单个portView视图，仅用于GraphView的GetCompatiblePorts函数使用（获取可连接端口）</param>
        public void RemovePortContentView(VisualElement portContentView, PortView portView)
        {
            CustomGraphView graphView = NodeDataPresenter.GetGraphAssetPresenter().GetGraphView();
            if (portView.direction == Direction.Input)
            {
                inputContainer.Remove(portContentView);
            }
            else
            {
                outputContainer.Remove(portContentView);
            }
            s_RemoveEdgesCache.Clear();
            s_RemoveEdgesCache.AddRange(portView.connections);
            foreach (Edge connection in s_RemoveEdgesCache)
            {
                connection.input.Disconnect(connection);
                connection.output.Disconnect(connection);
                graphView.RemoveElement(connection);
            }
            m_PortViews.Remove(portView);
        }

        public void ClearPortContentViews()
        {
            IReadOnlyList<BasePortDataPresenter> portDataPresenters = NodeDataPresenter.GetPortPresenters();
            CustomGraphView graphView = NodeDataPresenter.GetGraphAssetPresenter().GetGraphView();
            for (int i = 0; i < portDataPresenters.Count; i++)
            {
                PortView portView = portDataPresenters[i].GetPortView();
                s_RemoveEdgesCache.Clear();
                s_RemoveEdgesCache.AddRange(portView.connections);
                foreach (Edge connection in s_RemoveEdgesCache)
                {
                    connection.input.Disconnect(connection);
                    connection.output.Disconnect(connection);
                    graphView.RemoveElement(connection);
                }
            }
            m_PortViews.Clear();
            inputContainer.Clear();
            outputContainer.Clear();
        }

        public IReadOnlyList<PortView> GetPortViews()
        {
            return m_PortViews;
        }

        public PortView GetPortView(int portID)
        {
            for (int i = 0; i < m_PortViews.Count; i++)
            {
                if (m_PortViews[i].PortDataDataPresenter.GetPortData().GetPortID() == portID)
                {
                    return m_PortViews[i];
                }
            }
            return null;
        }

        public void RefreshPortContainerDisplay()
        {
            //TODO:对于输入输出只存在一个动态端口的情况时，初始动态端口没有任何值，但是在编辑完后会存在值，这时就需要显示这个端口（代理节点）
            inputContainer.style.display = inputContainer.childCount == 0 ? DisplayStyle.None : DisplayStyle.Flex;
            outputContainer.style.display = outputContainer.childCount == 0 ? DisplayStyle.None : DisplayStyle.Flex;
        }
    }
}