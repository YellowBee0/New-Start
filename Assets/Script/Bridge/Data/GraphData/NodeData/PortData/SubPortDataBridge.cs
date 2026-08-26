#if UNITY_EDITOR
using System;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using YBFramework.Bridge.Editor;
using YBFramework.GameLogic.Graph;

namespace YBFramework.Bridge.NewData
{
    [Serializable]
    public sealed class SubPortDataBridge : BasePortData
    {
        public string SubPortDisplayName;

        [SerializeField] private int m_PortID;

        [SerializeField] private PortConnectionData m_PortConnectionData;

        private BasePortData m_SubPortData;

        private BaseNodeData m_NodeData;

        private string m_PortName;

        private Direction m_Direction;

        private Color m_PortColor;

        public PortConnectionData GetSubPortAddress()
        {
            return m_PortConnectionData;
        }
        
        public BasePortData GetSubPortData()
        {
            return m_SubPortData;
        }

        public void SetSubPortData(BasePortData subPortData)
        {
            m_SubPortData = subPortData;
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
            Debug.LogWarning($"{nameof(SubPortDataBridge)} is useless to set a capacity:{capacity},because it is always {Port.Capacity.Single}");
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
            m_SubPortData = other;
            int subNodeID = other.GetNodeData().GetNodeID();
            int subPortID = other.GetPortID();
            m_PortConnectionData.NodeID = subNodeID;
            m_PortConnectionData.PortID = subPortID;
            SubPortDataBridgeConnectionChangeData.AddConnectionChangeData(GetNodeData().GetGraphAsset(), this, subNodeID, subPortID, true);
            other.BeConnected(this);
            other.SetHasSubPortData(true);
        }

        public override void Disconnect(BasePortData other)
        {
            int subNodeID = other.GetNodeData().GetNodeID();
            int subPortID = other.GetPortID();
            if (m_PortConnectionData.NodeID == subNodeID && m_PortConnectionData.PortID == subPortID)
            {
                m_SubPortData = null;
                m_PortConnectionData.NodeID = 0;
                m_PortConnectionData.PortID = 0;
                SubPortDataBridgeConnectionChangeData.AddConnectionChangeData(GetNodeData().GetGraphAsset(), this, subNodeID, subPortID, false);
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