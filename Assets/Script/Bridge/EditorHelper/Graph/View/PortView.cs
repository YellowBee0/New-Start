#if UNITY_EDITOR
using System;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using YBFramework.Bridge.Data;

namespace YBFramework.Bridge.Editor
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
                bool isInputCanConnectOutput = inputPortView.Port.CanConnect(outputPortView.Port);
                bool isOutputCanConnectInput = outputPortView.Port.CanConnect(inputPortView.Port);
                if (isInputCanConnectOutput ^ isOutputCanConnectInput)
                {
                    if (isInputCanConnectOutput)
                    {
                        Debug.LogError("Both port can connect other,we can not know witch one to connect");
                    }
                    return;
                }
                if (inputPortView.capacity == Capacity.Single)
                {
                    foreach (Edge connection in edge.input.connections)
                    {
                        if (connection != edge)
                        {
                            inputPortView.Disconnect(connection);
                        }
                    }
                }
                if (outputPortView.capacity == Capacity.Single)
                {
                    foreach (Edge connection in edge.output.connections)
                    {
                        if (connection != edge)
                        {
                            outputPortView.Disconnect(connection);
                        }
                    }
                }
                graphView.AddElement(edge);
                inputPortView.Connect(edge);
                outputPortView.Connect(edge);
                //判断是否为单连接，单连接需要断开连线在连接
            }
        }

        public readonly BasePortData Port;

        public readonly Action OnConnect;
        
        public readonly Action OnDisconnect;

        public PortView(BasePortData port, string name, Direction direction, Capacity capacity, Color color) : base(Orientation.Horizontal, direction, capacity, null)
        {
            Port = port;
            portName = name;
            portColor = color;
            m_EdgeConnector = new EdgeConnector<Edge>(new EdgeConnectorListener());
        }

        public override void Connect(Edge edge)
        {
            base.Connect(edge);
            OnConnect?.Invoke();
            //TODO:判断谁是当前对象谁是连接对象，在判断是谁连接谁
        }

        public override void Disconnect(Edge edge)
        {
            base.Disconnect(edge);
            OnDisconnect?.Invoke();
        }
    }
}
#endif