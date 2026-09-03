using System;
using System.Collections.Generic;
using YBFramework.Bridge.Data;

namespace YBFramework.Editor.NewGraph
{
    [RuntimeToEditor(typeof(BaseNodeData))]
    public class BaseNodeDrawer
    {
        private GraphAssetDrawer m_GraphAssetDrawer;

        private BaseNodeData m_NodeData;

        private NodeView m_NodeView;

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
        }

        public void RemovePortDrawer(BasePortDrawer portDrawer)
        {
            m_PortDrawers.Remove(portDrawer);
        }

        public void ClearPortDrawers()
        {
            for (int i = 0; i < m_PortDrawers.Count; i++)
            {
                BasePortDrawer.Release(m_PortDrawers[i]);
            }
            m_PortDrawers.Clear();
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
            m_NodeView = OnDrawNodeView(nodeData);
            m_NodeView.RefreshPortContainerDisplay();
            return m_NodeView;
        }

        protected virtual NodeView OnDrawNodeView(BaseNodeData nodeData)
        {
            NodeView nodeView = NodeView.Allocate(nodeData.GetNodeID(), this, nodeData.NodeName, nodeData.Position);
            int portDataCount = nodeData.GetPortsDataCount();
            for (int i = 0; i < portDataCount; i++)
            {
                BasePortData portData = nodeData.PortDataOfIndex(i);
                BasePortDrawer portDrawer = BasePortDrawer.Allocate(portData.GetType());
                if (portDrawer != null)
                {
                    nodeView.AddPortView(portDrawer.DrawPortView(this, portData));
                    AddPortDrawer(portDrawer);
                }
            }
            return nodeView;
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
                pool.Push(nodeDrawer);
            }
        }

        #endregion
    }
}