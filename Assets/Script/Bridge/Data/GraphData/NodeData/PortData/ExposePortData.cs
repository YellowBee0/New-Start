#if UNITY_EDITOR
using System;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using YBFramework.Bridge.Editor;
using YBFramework.GameLogic.Graph;

namespace YBFramework.Bridge.Data
{
    [Serializable]
    public sealed class ExposePortData : BasePortData
    {
        public string ExposePortDisplayName;

        [SerializeField] private int m_PortID;

        [SerializeField] private PortConnectionData m_PortConnectionData;

        private BasePortData m_ToExposePortData;

        private BaseNodeData m_NodeData;

        private string m_PortName;

        private Direction m_Direction;

        private Color m_PortColor;

        public PortConnectionData GetToExposePortAddress()
        {
            return m_PortConnectionData;
        }

        public BasePortData GetToExposePortData()
        {
            return m_ToExposePortData;
        }

        public void SetToExposePortData(BasePortData toExposePortData)
        {
            m_ToExposePortData = toExposePortData;
        }

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
            Debug.Log($"{nameof(ExposePortData)} is attempt to create a runtime instance,and this log is editor only");
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
            Debug.LogError($"{nameof(ExposePortData)} is not allowed to have sub port");
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
            m_Direction = direction;
        }

        public override void SetCapacity(Port.Capacity capacity)
        {
            Debug.LogWarning($"{nameof(ExposePortData)} is useless to set a capacity:{capacity},because it is always {Port.Capacity.Single}");
        }

        public override void SetPortColor(Color portColor)
        {
            m_PortColor = portColor;
        }

        public override void InitializeSerializedData()
        {
            m_PortConnectionData = new PortConnectionData();
        }

        public override BasePortData CreateSubPortData()
        {
            throw new InvalidOperationException($"{nameof(ExposePortData)} cannot create sub port");
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
            return base.CanConnect(other) && other is not ExposePortData;
        }

        public override void Connect(BasePortData other)
        {
            m_ToExposePortData = other;
            int toExposeNodeID = other.GetNodeData().GetNodeID();
            int toExposePortID = other.GetPortID();
            m_PortConnectionData.NodeID = toExposeNodeID;
            m_PortConnectionData.PortID = toExposePortID;
            ExposePortDataConnectionChangeData.AddConnectionChangeData(GetNodeData().GetGraphAsset(), this, toExposeNodeID, toExposePortID, true);
            other.BeConnected(this);
            other.SetHasSubPortData(true);
        }

        public override void Disconnect(BasePortData other)
        {
            int toExposeNodeID = other.GetNodeData().GetNodeID();
            int toExposePortID = other.GetPortID();
            if (m_PortConnectionData.NodeID == toExposeNodeID && m_PortConnectionData.PortID == toExposePortID)
            {
                m_ToExposePortData = null;
                m_PortConnectionData.NodeID = 0;
                m_PortConnectionData.PortID = 0;
                ExposePortDataConnectionChangeData.AddConnectionChangeData(GetNodeData().GetGraphAsset(), this, toExposeNodeID, toExposePortID, false);
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