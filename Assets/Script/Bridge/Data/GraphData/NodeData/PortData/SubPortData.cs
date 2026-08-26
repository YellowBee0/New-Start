using System;
using UnityEngine;
using YBFramework.GameLogic.Graph;
#if UNITY_EDITOR
using UnityEditor.Experimental.GraphView;
#endif

namespace YBFramework.Bridge.NewData
{
    [Serializable]
    public sealed class SubPortData : BasePortData
    {
        [SerializeReference] private BasePortData m_SubPortData;

        [SerializeField] private int m_SubNodeID;

        [SerializeField] private int m_SubPortID;

        public BasePortData GetSubPortData()
        {
            return m_SubPortData;
        }

        public int GetSubNodeID()
        {
            return m_SubNodeID;
        }

        public int GetSubPortID()
        {
            return m_SubPortID;
        }

        public override int GetPortID()
        {
            return m_SubPortData.GetPortID();
        }

        public override bool HasSubPortData()
        {
            return m_SubPortData.HasSubPortData();
        }

        public override int GetPortConnectionsDataCount()
        {
            return m_SubPortData.GetPortConnectionsDataCount();
        }

        public override PortConnectionData PortConnectionDataOfIndex(int index)
        {
            return m_SubPortData.PortConnectionDataOfIndex(index);
        }

        public override BasePort CreateRuntimeInstance()
        {
            throw new NotImplementedException();
        }
#if UNITY_EDITOR
        public SubPortData(BasePortData subPortData, int subNodeID, int subPortID)
        {
            m_SubPortData = subPortData;
            m_SubNodeID = subNodeID;
            m_SubPortID = subPortID;
        }

        public override BaseNodeData GetNodeData()
        {
            return m_SubPortData.GetNodeData();
        }

        public override void SetPortID(int portID)
        {
            m_SubPortData.SetPortID(portID);
        }

        public override void SetHasSubPortData(bool hasSubPortData)
        {
            m_SubPortData.SetHasSubPortData(hasSubPortData);
        }

        public override void SetNodeData(BaseNodeData nodeData)
        {
            m_SubPortData.SetNodeData(nodeData);
        }

        public override string GetPortName()
        {
            return m_SubPortData.GetPortName();
        }

        public override Direction GetDirection()
        {
            return m_SubPortData.GetDirection();
        }

        public override Port.Capacity GetCapacity()
        {
            return m_SubPortData.GetCapacity();
        }

        public override Color GetPortColor()
        {
            return m_SubPortData.GetPortColor();
        }

        public override void SetPortName(string portName)
        {
            m_SubPortData.SetPortName(portName);
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
            m_SubPortData.SetPortColor(portColor);
        }

        public override void InitializeSerializedData()
        {
            //这里不需要调用子端口的初始化，这个节点被创建出来时，子节点字段还没有被赋值，赋值后，子节点字段自己也调用了初始化
        }

        public override BasePortData CreateSubPortData()
        {
            return new SubPortData(m_SubPortData.CreateSubPortData(), GetNodeData().GetNodeID(), GetPortID());
        }

        public override void RevertNonSerializedData(BasePortData subSourcePortData)
        {
            SubPortData subPortData = (SubPortData)subSourcePortData;
            m_SubPortData.RevertNonSerializedData(subPortData);
        }

        public override int GetOtherPortConnectionsDataCount()
        {
            return m_SubPortData.GetOtherPortConnectionsDataCount();
        }

        public override PortConnectionData OtherPortConnectDataOfIndex(int index)
        {
            return m_SubPortData.OtherPortConnectDataOfIndex(index);
        }

        public override bool CanConnect(BasePortData other)
        {
            while (other is SubPortData subPortData)
            {
                other = subPortData.m_SubPortData;
            }
            return m_SubPortData.CanConnect(other);
        }

        public override void Connect(BasePortData other)
        {
            m_SubPortData.Connect(other);
        }

        public override void Disconnect(BasePortData other)
        {
            m_SubPortData.Disconnect(other);
        }

        public override void BeConnected(BasePortData other)
        {
            m_SubPortData.BeConnected(other);
        }

        public override void BeDisconnected(BasePortData other)
        {
            m_SubPortData.BeDisconnected(other);
        }
#endif
    }
}