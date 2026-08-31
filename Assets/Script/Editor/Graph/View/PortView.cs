using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace YBFramework.Editor.Graph
{
    public sealed class PortView : Port
    {
        public readonly BasePortDataPresenter PortDataDataPresenter;

        public PortView(BasePortDataPresenter portDataDataPresenter, string name, Direction direction, Capacity capacity, Color color) : base(Orientation.Horizontal, direction, capacity, null)
        {
            PortDataDataPresenter = portDataDataPresenter;
            portName = name;
            portColor = color;
            m_EdgeConnector = new EdgeConnector<Edge>(new EdgeConnectorListener());
            this.AddManipulator(m_EdgeConnector);
        }

        /// <summary>
        /// PortView视图上连接连线，如果数据也得修改，需要一起调用BindPortData的Connect函数
        /// </summary>
        /// <param name="edge">连线</param>
        public override void Connect(Edge edge)
        {
            base.Connect(edge);
            PortDataDataPresenter.OnPortViewConnect(edge);
            //不在这里持久化连线数据的原因：通过已经存在的连线数据恢复连线时也是调用这个函数，如果持久化数据会导致重复连接
        }

        /// <summary>
        /// PortView视图上断开连线，如果数据也得修改，需要一起调用BindPortData的Disconnect函数
        /// </summary>
        /// <param name="edge">连线</param>
        public override void Disconnect(Edge edge)
        {
            base.Disconnect(edge);
            PortDataDataPresenter.OnPortViewDisconnect(edge);
            //不在这里持久化连线数据的原因：为了对齐连接函数
        }
    }
}