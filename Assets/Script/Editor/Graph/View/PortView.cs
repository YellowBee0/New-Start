using System;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using YBFramework.Bridge.Data;

namespace YBFramework.Editor.Graph
{
    public sealed class PortView : Port
    {
        /// <summary>
        /// PortView视图绑定的BasePortData。
        /// 正常MVP架构是不允许数据Data和视图View之间有联系，但是为了用户操作视图时，能够快捷的获取到数据才这么做，不然只有去Presenter中一级一级查找非常耗时。
        /// View中只能读取Data数据，不能写入。比如BasePortData的Connect、Disconnect就不能调用，只能在Presenter里调用
        /// </summary>
        public readonly BasePortData BindPortData;

        private Action<Port> m_OnConnect;

        private Action<Port> m_OnDisconnect;

        public PortView(BasePortData portData, string name, Direction direction, Capacity capacity, Color color) : base(Orientation.Horizontal, direction, capacity, null)
        {
            BindPortData = portData;
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