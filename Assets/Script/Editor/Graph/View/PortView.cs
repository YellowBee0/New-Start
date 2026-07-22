using System;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace YBFramework.Editor
{
    public sealed class PortView : Port
    {
        private sealed class EdgeConnectorListener : IEdgeConnectorListener
        {
            public void OnDropOutsidePort(Edge edge, Vector2 position)
            {
            }

            public void OnDrop(GraphView graphView, Edge edge)
            {
                PortView inputPortView = (PortView)edge.input;
                PortView outputPortView = (PortView)edge.output;
                bool isInputCanConnectOutput = inputPortView.BindPortDrawer.GetBindPortData().CanConnect(outputPortView.BindPortDrawer.GetBindPortData());
                bool isOutputCanConnectInput = outputPortView.BindPortDrawer.GetBindPortData().CanConnect(inputPortView.BindPortDrawer.GetBindPortData());
                if (isInputCanConnectOutput ^ isOutputCanConnectInput)
                {
                    if (isInputCanConnectOutput)
                    {
                        Debug.LogError("This can not know witch one to connect because of both port can connect to other port");
                    }
                    return;
                }
                Edge singleConnectionToRemove = null;
                if (inputPortView.capacity == Capacity.Single)
                {
                    foreach (Edge connection in inputPortView.connections)
                    {
                        if (connection != edge)
                        {
                            singleConnectionToRemove = connection;

                            break;
                        }
                    }
                }
                if (outputPortView.capacity == Capacity.Single)
                {
                    foreach (Edge connection in outputPortView.connections)
                    {
                        if (connection != edge)
                        {
                            singleConnectionToRemove = connection;
                            break;
                        }
                    }
                }
                if (singleConnectionToRemove != null)
                {
                    PortView toRemoveInputPortView = (PortView)singleConnectionToRemove.input;
                    PortView toRemoveOutputPortView = (PortView)singleConnectionToRemove.output;
                    //区分不了这条连线是输入端口发起的连接还是输出端口发起的连接，所以直接调用两个的Disconnect
                    toRemoveInputPortView.BindPortDrawer.GetBindPortData().Disconnect(toRemoveOutputPortView.BindPortDrawer.GetBindPortData());
                    toRemoveOutputPortView.BindPortDrawer.GetBindPortData().Disconnect(toRemoveInputPortView.BindPortDrawer.GetBindPortData());
                    //视图上是必须两个端口都调用Disconnect
                    toRemoveInputPortView.Disconnect(singleConnectionToRemove);
                    toRemoveOutputPortView.Disconnect(singleConnectionToRemove);
                    graphView.RemoveElement(singleConnectionToRemove);
                }
                if (isInputCanConnectOutput)
                {
                    inputPortView.BindPortDrawer.GetBindPortData().Connect(outputPortView.BindPortDrawer.GetBindPortData());
                }
                else
                {
                    outputPortView.BindPortDrawer.GetBindPortData().Connect(inputPortView.BindPortDrawer.GetBindPortData());
                }
                inputPortView.Connect(edge);
                outputPortView.Connect(edge);
                graphView.AddElement(edge);
            }
        }

        public readonly BasePortDrawer BindPortDrawer;

        private Action m_OnConnect;

        private Action m_OnDisconnect;

        public PortView(string name, Direction direction, Capacity capacity, Color color, BasePortDrawer bindPortDrawer) : base(Orientation.Horizontal, direction, capacity, null)
        {
            BindPortDrawer = bindPortDrawer;
            portName = name;
            portColor = color;
            m_EdgeConnector = new EdgeConnector<Edge>(new EdgeConnectorListener());
            this.AddManipulator(m_EdgeConnector);
        }

        public void RegisterOnConnectCallback(Action onConnect)
        {
            if (onConnect != null)
            {
                m_OnConnect += onConnect;
            }
        }

        public void RegisterOnDisconnectCallback(Action onDisconnect)
        {
            if (onDisconnect != null)
            {
                m_OnDisconnect += onDisconnect;
            }
        }

        public void UnregisterOnConnectCallback(Action onConnect)
        {
            m_OnConnect -= onConnect;
        }

        public void UnregisterOnDisconnectCallback(Action onDisconnect)
        {
            m_OnDisconnect -= onDisconnect;
        }

        public void OnRelease()
        {
            BasePortDrawer.ReleasePortDrawer(BindPortDrawer);
        }

        /// <summary>
        /// PortView视图上连接连线，如果数据也得修改，需要一起调用BindPortData的Connect函数
        /// </summary>
        /// <param name="edge">连线</param>
        public override void Connect(Edge edge)
        {
            base.Connect(edge);
            m_OnConnect?.Invoke();
            //不在这里持久化连线数据的原因：通过已经存在的连线数据恢复连线时也是调用这个函数，如果持久化数据会导致重复连接
        }

        /// <summary>
        /// PortView视图上断开连线，如果数据也得修改，需要一起调用BindPortData的Disconnect函数
        /// </summary>
        /// <param name="edge">连线</param>
        public override void Disconnect(Edge edge)
        {
            base.Disconnect(edge);
            m_OnDisconnect?.Invoke();
            //不在这里持久化连线数据的原因：为了对齐连接函数
        }
    }
}