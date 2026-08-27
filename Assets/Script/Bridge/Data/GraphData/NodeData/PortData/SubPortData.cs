using System;
using UnityEngine;
using YBFramework.GameLogic.Graph;
#if UNITY_EDITOR
using UnityEditor.Experimental.GraphView;
#endif

namespace YBFramework.Bridge.Data
{
    [Serializable]
    public sealed class SubPortData : BasePortData
    {
        [SerializeReference] private BasePortData m_AsSubPortData;

        [SerializeField] private int m_AsSubNodeID;

        [SerializeField] private int m_AsSubPortID;

        public BasePortData GetAsSubPortData()
        {
            return m_AsSubPortData;
        }

        public int GetAsSubNodeID()
        {
            return m_AsSubNodeID;
        }

        public int GetAsSubPortID()
        {
            return m_AsSubPortID;
        }

        public override int GetPortID()
        {
            return m_AsSubPortData.GetPortID();
        }

        public override bool HasSubPortData()
        {
            return m_AsSubPortData.HasSubPortData();
        }

        public override int GetPortConnectionsDataCount()
        {
            return m_AsSubPortData.GetPortConnectionsDataCount();
        }

        public override PortConnectionData PortConnectionDataOfIndex(int index)
        {
            return m_AsSubPortData.PortConnectionDataOfIndex(index);
        }

        public override BasePort CreateRuntimeInstance()
        {
            throw new NotImplementedException();
        }
#if UNITY_EDITOR
        public SubPortData(BasePortData asSubPortData, int asSubNodeID, int asSubPortID)
        {
            m_AsSubPortData = asSubPortData;
            m_AsSubNodeID = asSubNodeID;
            m_AsSubPortID = asSubPortID;
        }

        public override BaseNodeData GetNodeData()
        {
            return m_AsSubPortData.GetNodeData();
        }

        public override void SetPortID(int portID)
        {
            m_AsSubPortData.SetPortID(portID);
        }

        public override void SetHasSubPortData(bool hasSubPortData)
        {
            m_AsSubPortData.SetHasSubPortData(hasSubPortData);
        }

        public override void SetNodeData(BaseNodeData nodeData)
        {
            m_AsSubPortData.SetNodeData(nodeData);
        }

        public override string GetPortName()
        {
            return m_AsSubPortData.GetPortName();
        }

        public override Direction GetDirection()
        {
            return m_AsSubPortData.GetDirection();
        }

        public override Port.Capacity GetCapacity()
        {
            return m_AsSubPortData.GetCapacity();
        }

        public override Color GetPortColor()
        {
            return m_AsSubPortData.GetPortColor();
        }

        public override void SetPortName(string portName)
        {
            m_AsSubPortData.SetPortName(portName);
        }

        public override void SetDirection(Direction direction)
        {
            Debug.LogWarning($"It's useless to set {nameof(SubPortData)}'s direction: {direction}, because the {nameof(SubPortData)}'s direction is limited by the sub port's direction");
        }

        public override void SetCapacity(Port.Capacity capacity)
        {
            Debug.LogWarning($"It's useless to set {nameof(SubPortData)}'s capacity: {capacity}, because the {nameof(SubPortData)}'s capacity is limited by the sub port's capacity");
        }

        public override void SetPortColor(Color portColor)
        {
            m_AsSubPortData.SetPortColor(portColor);
        }

        public override void InitializeSerializedData()
        {
        }

        public override BasePortData CreateSubPortData()
        {
            return new SubPortData(m_AsSubPortData.CreateSubPortData(), GetNodeData().GetNodeID(), GetPortID());
        }

        public override void RevertNonSerializedData(BasePortData subSourcePortData)
        {
            SubPortData subPortData = (SubPortData)subSourcePortData;
            m_AsSubPortData.RevertNonSerializedData(subPortData);
        }

        public override int GetOtherPortConnectionsDataCount()
        {
            return m_AsSubPortData.GetOtherPortConnectionsDataCount();
        }

        public override PortConnectionData OtherPortConnectDataOfIndex(int index)
        {
            return m_AsSubPortData.OtherPortConnectDataOfIndex(index);
        }

        public override bool CanConnect(BasePortData other)
        {
            while (other is SubPortData subPortData)
            {
                other = subPortData.m_AsSubPortData;
            }
            return m_AsSubPortData.CanConnect(other);
        }

        public override void Connect(BasePortData other)
        {
            m_AsSubPortData.Connect(other);
        }

        public override void Disconnect(BasePortData other)
        {
            m_AsSubPortData.Disconnect(other);
        }

        public override void BeConnected(BasePortData other)
        {
            m_AsSubPortData.BeConnected(other);
        }

        public override void BeDisconnected(BasePortData other)
        {
            m_AsSubPortData.BeDisconnected(other);
        }
#endif
    }
}