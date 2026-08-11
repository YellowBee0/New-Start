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

        public override int GetIndexPortConnectionDataCount()
        {
            return m_PortConnectionsData.Count;
        }

        public override PortConnectionData IndexPortConnectionData(int index)
        {
            return index < m_PortConnectionsData.Count ? m_PortConnectionsData[index] : null;
        }

        public override BasePort CreateRuntimeInstance()
        {
            //TODO:实现初始化逻辑
            return new ActionPort();
        }
#if UNITY_EDITOR
        private string m_PortName;

        private Direction m_Direction;

        private Port.Capacity m_Capacity;

        private Color m_PortColor;

        public override int GetSelfPortConnectionsDataCount()
        {
            return GetIndexPortConnectionDataCount();
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
            if (other is MethodPortData methodPortData)
            {
                base.Connect(other);
                SetIsUsed(true);
                other.SetIsUsed(true);
                bool isExplicitCast = methodPortData.GetReturnType() != typeof(void);
                m_PortConnectionsData.Add(new DelegatePortConnectionData
                {
                    NodeID = other.GetNodeData().GetNodeID(),
                    PortID = other.GetPortID(),
                    IsExplicitCast = isExplicitCast
                });
            }
        }

        public override void Disconnect(BasePortData other)
        {
            base.Disconnect(other);
            for (int i = 0; i < m_PortConnectionsData.Count; i++)
            {
                PortConnectionData portConnectionData = m_PortConnectionsData[i];
                if (portConnectionData.NodeID == other.GetNodeData().GetNodeID() && portConnectionData.PortID == other.GetPortID())
                {
                    m_PortConnectionsData.RemoveAt(i);
                    break;
                }
            }
            if (GetAllPortConnectionDataCount() == 0)
            {
                SetIsUsed(false);
            }
            if (other.GetAllPortConnectionDataCount() == 0)
            {
                other.SetIsUsed(false);
            }
        }

        public override BasePortData AsTemplate()
        {
            ActionPortData templateData = CreatePortData<ActionPortData>();
            templateData.m_PortID = m_PortID;
            return templateData;
        }

        public override void InitializeSerializedData()
        {
            base.InitializeSerializedData();
            m_PortConnectionsData = new List<DelegatePortConnectionData>();
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
#endif
    }
}