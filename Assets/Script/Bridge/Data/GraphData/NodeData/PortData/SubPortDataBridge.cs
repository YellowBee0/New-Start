#if UNITY_EDITOR
using System;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using YBFramework.GameLogic.Graph;

namespace YBFramework.Bridge.NewData
{
    [Serializable]
    public sealed class SubPortDataBridge : BasePortData
    {
        [SerializeField] private int m_PortID;

        [SerializeField] private PortConnectionData m_PortConnectionData;

        private BaseNodeData m_NodeData;

        private string m_PortName;

        private Direction m_Direction;

        private Port.Capacity m_Capacity;

        private Color m_PortColor;

        public override int GetPortID()
        {
            return m_PortID;
        }

        public override bool HasSubPortData()
        {
            return false;
        }

        public override int GetPortConnectionsDataCount()
        {
            return 1;
        }

        public override PortConnectionData PortConnectionDataOfIndex(int index)
        {
            return m_PortConnectionData;
        }

        public override BasePort CreateRuntimeInstance()
        {
            Debug.Log($"{nameof(SubPortDataBridge)} is attempt to create a runtime instance,and this log is editor only");
            return null;
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
            Debug.LogError($"{nameof(SubPortDataBridge)} is not allowed to have sub port");
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
            return m_Direction;
        }

        public override Port.Capacity GetCapacity()
        {
            return m_Capacity;
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
            m_Direction = direction;
        }

        public override void SetCapacity(Port.Capacity capacity)
        {
            m_Capacity = capacity;
        }

        public override void SetPortColor(Color portColor)
        {
            m_PortColor = portColor;
        }

        public override void InitializeSerializedData()
        {
        }

        public override BasePortData CreateSubPortData()
        {
            throw new InvalidOperationException($"{nameof(SubPortDataBridge)} cannot create sub port");
        }

        public override int GetOtherPortConnectionsDataCount()
        {
            return 0;
        }

        public override PortConnectionData OtherPortConnectDataOfIndex(int index)
        {
            return null;
        }

        public override bool CanConnect(BasePortData other)
        {
            return base.CanConnect(other) && other is not SubPortDataBridge;
        }

        public override void Connect(BasePortData other)
        {
            m_PortConnectionData.NodeID = other.GetNodeData().GetNodeID();
            m_PortConnectionData.PortID = other.GetPortID();
            other.BeConnected(this);
            other.SetHasSubPortData(true);
        }

        public override void Disconnect(BasePortData other)
        {
            if (m_PortConnectionData.NodeID == other.GetNodeData().GetNodeID() && m_PortConnectionData.PortID == other.GetPortID())
            {
                m_PortConnectionData.NodeID = 0;
                m_PortConnectionData.PortID = 0;
                other.BeDisconnected(this);
                other.SetHasSubPortData(false);
            }
        }

        public override void BeConnected(BasePortData other)
        {
        }

        public override void BeDisconnected(BasePortData other)
        {
        }
    }
}
#endif