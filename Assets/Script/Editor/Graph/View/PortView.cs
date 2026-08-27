using System;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace YBFramework.Editor.Graph
{
    public sealed class PortView : Port
    {
        public readonly BasePortDataPresenter BindPortDataDataPresenter;

        private Action<Port> m_OnConnect;

        private Action<Port> m_OnDisconnect;

        public PortView(BasePortDataPresenter bindPortDataDataPresenter, string name, Direction direction, Capacity capacity, Color color) : base(Orientation.Horizontal, direction, capacity, null)
        {
            BindPortDataDataPresenter = bindPortDataDataPresenter;
            portName = name;
            portColor = color;
            m_EdgeConnector = new EdgeConnector<Edge>(new EdgeConnectorListener());
            this.AddManipulator(m_EdgeConnector);
        }

        public void RegisterOnConnectCallback(Action<Port> onConnect)
        {
            if (onConnect != null)
            {
                m_OnConnect += onConnect;
            }
        }

        public void RegisterOnDisconnectCallback(Action<Port> onDisconnect)
        {
            if (onDisconnect != null)
            {
                m_OnDisconnect += onDisconnect;
            }
        }

        public void UnregisterOnConnectCallback(Action<Port> onConnect)
        {
            m_OnConnect -= onConnect;
        }

        public void UnregisterOnDisconnectCallback(Action<Port> onDisconnect)
        {
            m_OnDisconnect -= onDisconnect;
        }


        /// <summary>
        /// PortView视图上连接连线，如果数据也得修改，需要一起调用BindPortData的Connect函数
        /// </summary>
        /// <param name="edge">连线</param>
        public override void Connect(Edge edge)
        {
            base.Connect(edge);
            m_OnConnect?.Invoke(direction == Direction.Input ? edge.output : edge.input);
            //不在这里持久化连线数据的原因：通过已经存在的连线数据恢复连线时也是调用这个函数，如果持久化数据会导致重复连接
        }

        /// <summary>
        /// PortView视图上断开连线，如果数据也得修改，需要一起调用BindPortData的Disconnect函数
        /// </summary>
        /// <param name="edge">连线</param>
        public override void Disconnect(Edge edge)
        {
            base.Disconnect(edge);
            m_OnDisconnect?.Invoke(direction == Direction.Input ? edge.output : edge.input);
            //不在这里持久化连线数据的原因：为了对齐连接函数
        }
    }
}