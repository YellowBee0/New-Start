using System;
using System.Collections.Generic;
using UnityEngine;
using YBFramework.GameLogic.Graph;
#if UNITY_EDITOR
using UnityEditor.Experimental.GraphView;
#endif

namespace YBFramework.Bridge.NewData
{
    [Serializable]
    public sealed class ActionPortData : BasePortData
    {
        [SerializeField] private List<DelegatePortConnectionData> m_PortConnectionsData;

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
            return m_PortConnectionsData.Count;
        }

        public override PortConnectionData PortConnectionDataOfIndex(int index)
        {
            return m_PortConnectionsData[index];
        }

        public override BasePort CreateRuntimeInstance()
        {
            throw new NotImplementedException();
        }

        public override void LinkOtherPort(CheckValidStack checkValidStack, NodeDataOnCallChain validNodesData)
        {
            for (int i = 0; i < m_PortConnectionsData.Count; i++)
            {
                PortConnectionData portConnectionData = m_PortConnectionsData[i];
                BaseNodeData nodeData = checkValidStack.GetCurrentGraphAsset().FindNodeData(portConnectionData.NodeID);
                if (nodeData != null)
                {
                    nodeData.LinkOtherPort(checkValidStack, validNodesData);
                }
            }
            if (m_HasSubPortData)
            {
                CheckValidStack parentCheckValidStack = checkValidStack.GetParentStack();
                if (parentCheckValidStack != null)
                {
                    SubNodeData parentNodeData = (SubNodeData)parentCheckValidStack.GetCurrentNodeData();
                    parentNodeData.LinkOtherPort(parentCheckValidStack, null, parentNodeData.GetSubPortDataBySubPortID(GetPortID()));
                }
            }
        }
#if UNITY_EDITOR
        [SerializeField] private List<PortConnectionData> m_OtherPortConnectionsData;

        private BaseNodeData m_NodeData;

        private string m_PortName;

        private Direction m_Direction;

        private Port.Capacity m_Capacity;

        private Color m_PortColor;

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
            m_PortConnectionsData = new List<DelegatePortConnectionData>();
            m_OtherPortConnectionsData = new List<PortConnectionData>();
        }

        public override BasePortData CreateSubPortData()
        {
            ActionPortData portData = new();
            portData.InitializeSerializedData();
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
            if (base.CanConnect(other))
            {
                if (other is MethodPortData methodPortData)
                {
                    if (methodPortData.GetParameters().Length == 0)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public override void Connect(BasePortData other)
        {
            MethodPortData methodPortData = (MethodPortData)other;
            bool isExplicitCast = methodPortData.GetReturnType() != typeof(void);
            m_PortConnectionsData.Add(new DelegatePortConnectionData
            {
                NodeID = other.GetNodeData().GetNodeID(),
                PortID = other.GetPortID(),
                IsExplicitCast = isExplicitCast
            });
        }

        public override void Disconnect(BasePortData other)
        {
            for (int i = 0; i < m_PortConnectionsData.Count; i++)
            {
                PortConnectionData portConnectionData = m_PortConnectionsData[i];
                if (portConnectionData.NodeID == other.GetNodeData().GetNodeID() && portConnectionData.PortID == other.GetPortID())
                {
                    m_PortConnectionsData.RemoveAt(i);
                    break;
                }
            }
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