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
                bool isInputCanConnectOutput = inputPortView.BindPortData.CanConnect(outputPortView.BindPortData);
                bool isOutputCanConnectInput = outputPortView.BindPortData.CanConnect(inputPortView.BindPortData);
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
            }
        }

        public readonly BasePortData BindPortData;

        private Action m_OnConnect;

        private Action m_OnDisconnect;

        public PortView(BasePortData bindPortData, string name, Direction direction, Capacity capacity, Color color) : base(Orientation.Horizontal, direction, capacity, null)
        {
            BindPortData = bindPortData;
            portName = name;
            portColor = color;
            m_EdgeConnector = new EdgeConnector<Edge>(new EdgeConnectorListener());
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
        
        public override void Connect(Edge edge)
        {
            base.Connect(edge);
            m_OnConnect?.Invoke();
            //TODO:判断谁是当前对象谁是连接对象，在判断是谁连接谁。在这里添加连线数据
        }

        public override void Disconnect(Edge edge)
        {
            base.Disconnect(edge);
            m_OnDisconnect?.Invoke();
            //TODO:在这移除连线数据
        }
    }
}
#endif