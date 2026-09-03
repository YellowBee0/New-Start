using System;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace YBFramework.Editor.NewGraph
{
    public sealed class PortView : Port
    {
        private int m_PortID;

        /// <summary>
        /// 只有缓存Drawer，不然每次视图上用户操作了，不能方便地获取到操作的数据
        /// </summary>
        private BasePortDrawer m_PortDrawer;

        private Action<Edge> m_OnPortViewConnect;

        private Action<Edge> m_OnPortViewDisconnect;

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

        public void ClearConnections()
        {
            //之后Edge可能会更改为EdgeView，也会池化
            //TODO:这里edge被删除了还需要在GraphView中删除和在双方PortView删除
            m_Edges.Clear();
            m_Edges.AddRange(connections);
            foreach (Edge edge in m_Edges)
            {
                Disconnect(edge);
            }
        }

        public void SetEdgeConnector(EdgeConnector<EdgeView> portEdgeConnector)
        {
            this.RemoveManipulator(m_EdgeConnector);
            m_EdgeConnector = portEdgeConnector;
            this.AddManipulator(portEdgeConnector);
        }

        public void RegisterOnPortViewConnect(Action<Edge> onPortViewConnect)
        {
            m_OnPortViewConnect += onPortViewConnect;
        }

        public void UnregisterOnPortViewConnect(Action<Edge> onPortViewConnect)
        {
            m_OnPortViewConnect -= onPortViewConnect;
        }

        public void RegisterOnPortViewDisconnect(Action<Edge> onPortViewDisconnect)
        {
            m_OnPortViewDisconnect += onPortViewDisconnect;
        }

        public void UnregisterOnPortViewDisconnect(Action<Edge> onPortViewDisconnect)
        {
            m_OnPortViewDisconnect -= onPortViewDisconnect;
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

        public override void Connect(Edge edge)
        {
            base.Connect(edge);
            m_OnPortViewConnect?.Invoke(edge);
        }

        public override void Disconnect(Edge edge)
        {
            base.Disconnect(edge);
            m_OnPortViewDisconnect?.Invoke(edge);
        }

        private static readonly List<Edge> m_Edges = new();

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
                pool.Push(portView);
            }
        }

        #endregion
    }
}