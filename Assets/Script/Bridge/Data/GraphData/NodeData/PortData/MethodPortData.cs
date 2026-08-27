using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using YBFramework.GameLogic.Graph;

namespace YBFramework.Bridge.Data
{
    [Serializable]
    public sealed class MethodPortData : BasePortData
    {
        private MethodInfo m_MethodInfo;

        [SerializeField] private int m_PortID;

        [SerializeField] private bool m_HasSubPortData;

        public MethodInfo GetMethodInfo()
        {
            return m_MethodInfo;
        }

        public void SetMethodInfo(MethodInfo methodInfo)
        {
            m_MethodInfo = methodInfo;
#if UNITY_EDITOR
            if (methodInfo != null)
            {
                m_ParameterInfos = methodInfo.GetParameters();
                m_ReturnType = methodInfo.ReturnType;
            }
#endif
        }

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
            return 0;
        }

        public override PortConnectionData PortConnectionDataOfIndex(int index)
        {
            return null;
        }

        public override BasePort CreateRuntimeInstance()
        {
            throw new NotImplementedException();
        }
#if UNITY_EDITOR

        [SerializeField] private List<PortConnectionData> m_OtherPortConnectionsData;

        private Type m_ReturnType;

        private ParameterInfo[] m_ParameterInfos;

        private BaseNodeData m_NodeData;

        private string m_PortName;

        private Direction m_Direction;

        private Port.Capacity m_Capacity;

        private Color m_PortColor;

        public Type GetReturnType()
        {
            return m_ReturnType;
        }

        public ParameterInfo[] GetParameters()
        {
            return m_ParameterInfos;
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
            m_OtherPortConnectionsData = new List<PortConnectionData>();
        }

        public override BasePortData CreateSubPortData()
        {
            MethodPortData portData = new();
            portData.InitializeSerializedData();
            return portData;
        }

        public override void RevertNonSerializedData(BasePortData subSourcePortData)
        {
            base.RevertNonSerializedData(subSourcePortData);
            MethodPortData data = (MethodPortData)subSourcePortData;
            m_MethodInfo = data.m_MethodInfo;
            m_ParameterInfos = data.m_ParameterInfos;
            m_ReturnType = data.m_ReturnType;
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
            return false;
        }

        public override void Connect(BasePortData other)
        {
            Debug.LogWarning($"{nameof(MethodPortData)} cannot actively connect to any other port.");
        }

        public override void Disconnect(BasePortData other)
        {
            Debug.LogWarning($"{nameof(MethodPortData)} cannot actively disconnect to any other port.");
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