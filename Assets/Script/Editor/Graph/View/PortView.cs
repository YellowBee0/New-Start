using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using YBFramework.Bridge.Data;

namespace YBFramework.Editor.Graph
{
    public sealed class PortView : Port
    {
        private int m_PortID;

        /// <summary>
        /// 只有缓存Drawer，不然每次视图上用户操作了，不能方便地获取到操作的数据
        /// </summary>
        private BasePortDrawer m_PortDrawer;

        private PortView(Direction direction, Capacity capacity) : base(Orientation.Horizontal, direction, capacity, null)
        {
        }

        public int GetPortID()
        {
            return m_PortID;
        }

        public BasePortDrawer GetPortDrawer()
        {
            return m_PortDrawer;
        }

        public void SetEdgeConnector(EdgeConnector<EdgeView> portEdgeConnector)
        {
            this.RemoveManipulator(m_EdgeConnector);
            m_EdgeConnector = portEdgeConnector;
            this.AddManipulator(portEdgeConnector);
        }

        public Edge FindConnection(PortView other)
        {
            Edge connection = null;
            foreach (Edge edge in connections)
            {
                if (direction == Direction.Input)
                {
                    if (edge.output == other)
                    {
                        connection = edge;
                        break;
                    }
                }
                else
                {
                    if (edge.input == other)
                    {
                        connection = edge;
                        break;
                    }
                }
            }
            return connection;
        }

        public void RevertPortViewConnections()
        {
            BasePortData portData = m_PortDrawer.GetPortData();
            CustomGraphView graphView = m_PortDrawer.GetNodeDrawer().GetGraphAssetDrawer().GetGraphView();
            int portConnectionsDataCount = portData.GetPortConnectionsDataCount();
            for (int i = 0; i < portConnectionsDataCount; i++)
            {
                PortConnectionData portConnectionData = portData.PortConnectionDataOfIndex(i);
                if (portConnectionData.IsValid())
                {
                    NodeView toNodeView = graphView.FindNodeView(portConnectionData.NodeID);
                    if (toNodeView != null)
                    {
                        PortView toPortView = toNodeView.FindPortView(portConnectionData.PortID);
                        if (toPortView != null)
                        {
                            CustomGraphView.Connect(this, toPortView, graphView);
                        }
                    }
                }
            }
        }

        private void OnRelease()
        {
            CustomGraphView.DisconnectAll(this, m_PortDrawer.GetNodeDrawer().GetGraphAssetDrawer().GetGraphView());
        }

        public override void Connect(Edge edge)
        {
            base.Connect(edge);
            m_PortDrawer.OnPortViewConnect(edge);
        }

        public override void Disconnect(Edge edge)
        {
            base.Disconnect(edge);
            m_PortDrawer.OnPortViewDisconnect(edge);
        }

        #region Pool

        private static readonly Dictionary<(Direction, Capacity), Stack<PortView>> s_Pools = new();

        public static PortView Allocate(Direction direction, Capacity capacity, int portID, BasePortDrawer portDrawer, string portViewName, Color color)
        {
            (Direction direction, Capacity capacity) key = (direction, capacity);
            if (!s_Pools.TryGetValue(key, out Stack<PortView> pool))
            {
                pool = new Stack<PortView>();
                s_Pools.Add(key, pool);
            }
            PortView portView = pool.Count > 0 ? pool.Pop() : new PortView(direction, capacity);
            portView.m_PortID = portID;
            portView.m_PortDrawer = portDrawer;
            portView.portName = portViewName;
            portView.portColor = color;
            return portView;
        }

        public static void Release(PortView portView)
        {
            (Direction direction, Capacity capacity) key = (portView.direction, portView.capacity);
            if (s_Pools.TryGetValue(key, out Stack<PortView> pool))
            {
                portView.OnRelease();
                pool.Push(portView);
            }
        }

        #endregion
    }
}