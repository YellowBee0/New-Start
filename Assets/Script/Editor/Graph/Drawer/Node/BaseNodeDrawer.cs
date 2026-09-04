using System;
using System.Collections.Generic;
using YBFramework.Bridge.Data;

namespace YBFramework.Editor.Graph
{
    [RuntimeToEditor(typeof(BaseNodeData))]
    public class BaseNodeDrawer
    {
        protected GraphAssetDrawer m_GraphAssetDrawer;

        protected BaseNodeData m_NodeData;

        protected NodeView m_NodeView;

        private readonly List<BasePortDrawer> m_PortDrawers = new();

        public GraphAssetDrawer GetGraphAssetDrawer()
        {
            return m_GraphAssetDrawer;
        }

        public BaseNodeData GetNodeData()
        {
            return m_NodeData;
        }

        public NodeView GetNodeView()
        {
            return m_NodeView;
        }

        public IReadOnlyList<BasePortDrawer> GetPortDrawers()
        {
            return m_PortDrawers;
        }

        public void AddPortDrawer(BasePortDrawer portDrawer)
        {
            m_PortDrawers.Add(portDrawer);
            m_NodeView.AddPortView(portDrawer.GetPortView());
        }

        public void RemovePortDrawer(BasePortDrawer portDrawer)
        {
            if (m_PortDrawers.Remove(portDrawer))
            {
                BasePortDrawer.Release(portDrawer);
            }
            m_NodeView.RemovePortView(portDrawer.GetPortView());
        }

        public void ClearPortDrawers()
        {
            for (int i = 0; i < m_PortDrawers.Count; i++)
            {
                BasePortDrawer.Release(m_PortDrawers[i]);
            }
            m_NodeView.ClearPortViews();
        }

        /// <summary>
        /// 绘制节点全部端口，仅在初始绘制节点视图或者需要重新绘制节点端口视图时调用
        /// </summary>
        public void DrawPortViews()
        {
            int portDataCount = m_NodeData.GetPortsDataCount();
            for (int i = 0; i < portDataCount; i++)
            {
                BasePortData portData = m_NodeData.PortDataOfIndex(i);
                DrawPortView(portData);
            }
        }

        public void DrawPortView(BasePortData portData)
        {
            BasePortDrawer portDrawer = BasePortDrawer.Allocate(portData.GetType());
            if (portDrawer != null)
            {
                portDrawer.DrawPortView(this, portData);
                AddPortDrawer(portDrawer);
            }
        }

        public BasePortDrawer FindPortDrawer(int portID)
        {
            for (int i = 0; i < m_PortDrawers.Count; i++)
            {
                BasePortDrawer portDrawer = m_PortDrawers[i];
                if (portDrawer.GetPortData().GetPortID() == portID)
                {
                    return portDrawer;
                }
            }
            return null;
        }

        public NodeView DrawNodeView(GraphAssetDrawer graphAssetDrawer, BaseNodeData nodeData)
        {
            m_GraphAssetDrawer = graphAssetDrawer;
            nodeData.InitializePortData();
            m_NodeData = nodeData;
            m_NodeView = NodeView.Allocate(m_NodeData.GetNodeID(), this, m_NodeData.NodeName, m_NodeData.Position);
            OnDrawNodeView();
            m_NodeView.RefreshPortContainerDisplay();
            return m_NodeView;
        }

        protected virtual void OnDrawNodeView()
        {
            DrawPortViews();
        }

        protected virtual void OnRelease()
        {
            //这里不能调用ClearPortDrawers，因为ClearPortDrawers会调用NodeView的ClearPortViews，ClearPortViews又会调用PortView.Release
            //这个释放函数本身就和NodeView的Release一起调用，调用ClearPortViews就会导致重复
            for (int i = 0; i < m_PortDrawers.Count; i++)
            {
                BasePortDrawer.Release(m_PortDrawers[i]);
            }
            m_PortDrawers.Clear();
        }

        #region Pool

        private static readonly Dictionary<Type, Stack<BaseNodeDrawer>> s_Pools = new();

        public static BaseNodeDrawer Allocate(Type nodeDataType)
        {
            Type nodeDrawerType = RuntimeToEditorMap.GetInstance().GetDrawerType(nodeDataType);
            if (nodeDrawerType == null)
            {
                return null;
            }
            if (!s_Pools.TryGetValue(nodeDrawerType, out Stack<BaseNodeDrawer> pool))
            {
                pool = new Stack<BaseNodeDrawer>();
                s_Pools.Add(nodeDrawerType, pool);
            }
            return pool.Count > 0 ? pool.Pop() : Activator.CreateInstance(nodeDrawerType) as BaseNodeDrawer;
        }

        public static void Release(BaseNodeDrawer nodeDrawer)
        {
            if (s_Pools.TryGetValue(nodeDrawer.GetType(), out Stack<BaseNodeDrawer> pool))
            {
                nodeDrawer.OnRelease();
                pool.Push(nodeDrawer);
            }
        }

        #endregion
    }
}