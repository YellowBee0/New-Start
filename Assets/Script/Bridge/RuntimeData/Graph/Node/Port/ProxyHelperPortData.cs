#if UNITY_EDITOR
using System;
using UnityEngine;
using YBFramework.GameLogic.Graph;

namespace YBFramework.Bridge.Data
{
    [Serializable]
    public sealed class ProxyHelperPortData : BasePortData
    {
        //TODO:这个被创建出来时直接使用代理目标端口的name
        public string ProxyName;

        public PortConnectionData TargetPortConnectionData;

        private BasePortData m_TargetPortData;

        public BasePortData GetTargetPortData()
        {
            return m_TargetPortData;
        }

        public void SetTargetPortData(BasePortData targetPortData)
        {
            m_TargetPortData = targetPortData;
        }

        public override BasePort CreateRuntimeInstance()
        {
            Debug.Log("Editor only port:proxy helper port is tried to create a runtime port");
            return null;
        }

        public override PortConnectionData GetPortConnectionDataFromSelf(int nodeId, int portId)
        {
            if (TargetPortConnectionData.NodeID == nodeId && TargetPortConnectionData.PortID == portId)
            {
                return TargetPortConnectionData;
            }
            return null;
        }

        public override int GetPortConnectionDataCountFromSelf()
        {
            if (TargetPortConnectionData.NodeID != 0 && TargetPortConnectionData.PortID != 0)
            {
                return 1;
            }
            return 0;
        }

        public override bool CanConnect(BasePortData other)
        {
            return base.CanConnect(other) && other is not ProxyHelperPortData;
        }

        public override void Connect(BasePortData other)
        {
            base.Connect(other);
            TargetPortConnectionData.NodeID = other.GetNodeData().NodeID;
            TargetPortConnectionData.PortID = other.PortID;
        }

        public override void Disconnect(BasePortData other)
        {
            base.Disconnect(other);
            if (TargetPortConnectionData.NodeID == other.GetNodeData().NodeID && TargetPortConnectionData.PortID == other.PortID)
            {
                TargetPortConnectionData.NodeID = 0;
                TargetPortConnectionData.PortID = 0;
            }
            if (GetPortConnectionDataCount() == 0)
            {
                IsUsed = false;
            }
        }

        public override BasePortData Clone()
        {
            throw new Exception("this port can not clone for proxy port");
        }
    }
}
#endif