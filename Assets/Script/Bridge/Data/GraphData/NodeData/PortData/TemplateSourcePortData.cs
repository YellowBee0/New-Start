using System;
using UnityEngine;
using YBFramework.GameLogic.Graph;
#if UNITY_EDITOR
using UnityEditor.Experimental.GraphView;
#endif

namespace YBFramework.Bridge.NewData
{
    [Serializable]
    public sealed class TemplateSourcePortData : BasePortData
    {
        [SerializeField] private string m_DisplayName;

        [SerializeField] private PortConnectionData m_PortConnectionData;

        private BasePortData m_SourcePortData;

        public PortConnectionData GetValidSourcePortDataAddress()
        {
            if (m_SourcePortData == null || m_PortConnectionData.NodeID == 0 || m_PortConnectionData.PortID == 0)
            {
                return null;
            }
            return m_PortConnectionData;
        }

        public override int GetIndexPortConnectionDataCount()
        {
            return 1;
        }

        public override PortConnectionData IndexPortConnectionData(int index)
        {
            return index == 0 ? m_PortConnectionData : null;
        }

        public override BasePort CreateRuntimeInstance()
        {
            Debug.Log($"{nameof(TemplateSourcePortData)} port attempts to create an instance.");
            return null;
        }
#if UNITY_EDITOR
        private string m_PortName;

        private Direction m_Direction;

        private Color m_PortColor;

        public BasePortData GetSourcePortData()
        {
            if (m_SourcePortData == null && GetValidSourcePortDataAddress() != null)
            {
                //正常不为null
                BaseNodeData nodeData = m_NodeData.GetGraphAsset().FindNodeData(m_PortConnectionData.NodeID);
                m_SourcePortData = nodeData.FindPortData(m_PortConnectionData.PortID);
            }
            return m_SourcePortData;
        }

        public override int GetSelfPortConnectionsDataCount()
        {
            return GetValidSourcePortDataAddress() == null ? 0 : 1;
        }

        public override bool CanConnect(BasePortData other)
        {
            return base.CanConnect(other) && other is not TemplateSourcePortData;
        }

        public override void Connect(BasePortData other)
        {
            base.Connect(other);
            m_PortConnectionData.NodeID = other.GetNodeData().GetNodeID();
            m_PortConnectionData.PortID = other.GetPortID();
        }

        public override BasePortData AsTemplate()
        {
            throw new InvalidOperationException($"{nameof(TemplateSourcePortData)} port cannot as a template port");
        }

        public override void CopyNonSerializedData(BasePortData templateData)
        {
            Debug.LogError($"Graph: {templateData.GetNodeData().GetGraphAsset().name} has make an {nameof(TemplateSourcePortData)} port as a template port");
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
            Debug.LogWarning($"{nameof(TemplateSourcePortData)} port's capacity is always single");
        }

        public override void SetPortColor(Color portColor)
        {
            m_PortColor = portColor;
        }
#endif
    }
}