using System;
using System.Collections.Generic;
using UnityEngine;
using YBFramework.GameLogic.Graph;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Experimental.GraphView;
#endif

namespace YBFramework.Bridge.Data
{
    [Serializable]
    public sealed class ValueInputPortData<TValue> : BasePortData
    {
        [SerializeField] private TValue m_Value;

        [SerializeField] private DelegatePortConnectionData m_DelegatePortConnectionData;

        [SerializeField] private int m_PortID;

        [SerializeField] private bool m_HasSubPortData;

        public override int GetPortID()
        {
            return m_PortID;
        }

        public override bool HasSubPortData()
        {
            return m_HasSubPortData;
        }

        public override int GetPortConnectionsDataCount()
        {
            return 1;
        }

        public override PortConnectionData PortConnectionDataOfIndex(int index)
        {
            return index == 0 ? m_DelegatePortConnectionData : null;
        }

        public override BasePort CreateRuntimeInstance()
        {
            throw new NotImplementedException();
        }
#if UNITY_EDITOR
        [SerializeField] private List<PortConnectionData> m_OtherPortConnectionsData;

        private BaseNodeData m_NodeData;

        private string m_PortName;

        private Color m_PortColor;

        public void SetValue(TValue value)
        {
            m_Value = value;
        }

        public TValue GetValue()
        {
            return m_Value;
        }

        public override BaseNodeData GetNodeData()
        {
            return m_NodeData;
        }

        public override void SetPortID(int portID)
        {
            m_PortID = portID;
        }

        public override void SetHasSubPortData(bool hasSubPortData)
        {
            m_HasSubPortData = hasSubPortData;
        }

        public override void SetNodeData(BaseNodeData nodeData)
        {
            m_NodeData = nodeData;
        }

        public override string GetPortName()
        {
            return m_PortName;
        }

        public override Direction GetDirection()
        {
            return Direction.Input;
        }

        public override Port.Capacity GetCapacity()
        {
            return Port.Capacity.Single;
        }

        public override Color GetPortColor()
        {
            return m_PortColor;
        }

        public override void SetPortName(string portName)
        {
            m_PortName = portName;
        }

        public override void SetDirection(Direction direction)
        {
            Debug.LogWarning($"{nameof(ValueInputPort<TValue>)} is useless to set a direction:{direction},because it is always {Direction.Input}");
        }

        public override void SetCapacity(Port.Capacity capacity)
        {
            Debug.LogWarning($"{nameof(ValueInputPort<TValue>)} is useless to set a capacity:{capacity},because it is always {Port.Capacity.Single}");
        }

        public override void SetPortColor(Color portColor)
        {
            m_PortColor = portColor;
        }

        public override void InitializeSerializedData()
        {
            m_OtherPortConnectionsData = new List<PortConnectionData>();
        }

        public override BasePortData CreateSubPortData()
        {
            ValueInputPortData<TValue> portData = new();
            string json = EditorJsonUtility.ToJson(this);
            EditorJsonUtility.FromJsonOverwrite(json, portData);
            portData.m_OtherPortConnectionsData.Clear();
            portData.m_DelegatePortConnectionData.NodeID = 0;
            portData.m_DelegatePortConnectionData.PortID = 0;
            return portData;
        }

        public override int GetOtherPortConnectionsDataCount()
        {
            return m_OtherPortConnectionsData.Count;
        }

        public override PortConnectionData OtherPortConnectDataOfIndex(int index)
        {
            return m_OtherPortConnectionsData[index];
        }

        public override bool CanConnect(BasePortData other)
        {
            if (base.CanConnect(other) && other is MethodPortData methodPortData)
            {
                return methodPortData.GetParameters().Length == 0 && typeof(TValue).IsAssignableFrom(methodPortData.GetReturnType());
            }
            return false;
        }

        public override void Connect(BasePortData other)
        {
            MethodPortData methodPort = (MethodPortData)other;
            Type valueType = typeof(TValue);
            Type returnType = methodPort.GetReturnType();
            bool isExplicitCast = returnType.IsValueType && returnType != valueType;
            m_DelegatePortConnectionData = new DelegatePortConnectionData
            {
                NodeID = other.GetNodeData().GetNodeID(),
                PortID = other.GetPortID(),
                IsExplicitCast = isExplicitCast
            };
            other.BeConnected(this);
        }

        public override void Disconnect(BasePortData other)
        {
            if (m_DelegatePortConnectionData.NodeID == other.GetNodeData().GetNodeID() && m_DelegatePortConnectionData.PortID == other.GetPortID())
            {
                m_DelegatePortConnectionData.NodeID = 0;
                m_DelegatePortConnectionData.PortID = 0;
            }
            other.BeConnected(this);
        }

        public override void BeConnected(BasePortData other)
        {
            m_OtherPortConnectionsData.Add(new PortConnectionData
            {
                NodeID = other.GetNodeData().GetNodeID(),
                PortID = other.GetPortID()
            });
        }

        public override void BeDisconnected(BasePortData other)
        {
            for (int i = 0; i < m_OtherPortConnectionsData.Count; i++)
            {
                PortConnectionData portConnectionData = m_OtherPortConnectionsData[i];
                if (portConnectionData.NodeID == other.GetNodeData().GetNodeID() && portConnectionData.PortID == other.GetPortID())
                {
                    m_OtherPortConnectionsData.RemoveAt(i);
                    return;
                }
            }
        }
#endif
    }
}