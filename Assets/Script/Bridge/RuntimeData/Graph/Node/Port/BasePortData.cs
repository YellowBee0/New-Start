using System;
using YBFramework.Common;
using YBFramework.GameLogic.Graph;
#if UNITY_EDITOR
using YBFramework.EditorOnly;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
#endif

namespace YBFramework.Bridge
{
    [Serializable]
    public abstract class BasePortData : IRuntimeData<BasePort>, IValueIterator<PortConnectionData>
    {
        public int PortID;

        public abstract BasePort CreateRuntimeInstance();

        public virtual bool Iterator(int index, out PortConnectionData current)
        {
            current = null;
            return false;
        }
#if UNITY_EDITOR
        [SerializeField] private List<PortConnectionData> m_PortConnectionDataFromOther;

        protected BaseNodeData m_NodeData;

        protected PortViewArgs m_PortViewArgs;

        public PortViewArgs GetPortViewArgs()
        {
            return m_PortViewArgs;
        }

        public void SetPortViewArgs(PortViewArgs args)
        {
            SetPortViewArgs(args.Name, args.Direction, args.Capacity, args.Color);
        }

        public void SetPortViewArgs(string name, PortViewArgsTemplate argsTemplate)
        {
            Direction direction;
            Port.Capacity capacity;
            Color color;
            switch (argsTemplate)
            {
                case PortViewArgsTemplate.None:
                    direction = default;
                    capacity = default;
                    color = default;
                    break;
                case PortViewArgsTemplate.LogicInput:
                    direction = Direction.Input;
                    capacity = Port.Capacity.Multi;
                    color = Color.red;
                    break;
                case PortViewArgsTemplate.LogicOutput:
                    direction = Direction.Output;
                    capacity = Port.Capacity.Multi;
                    color = Color.red;
                    break;
                case PortViewArgsTemplate.ValueInput:
                    direction = Direction.Input;
                    capacity = Port.Capacity.Single;
                    color = Color.blue;
                    break;
                case PortViewArgsTemplate.ValueOutput:
                    direction = Direction.Output;
                    capacity = Port.Capacity.Multi;
                    color = Color.blue;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(argsTemplate), argsTemplate, null);
            }
            SetPortViewArgs(name, direction, capacity, color);
        }

        public PortConnectionData GetPortConnectionData(int nodeId, int portId)
        {
            PortConnectionData portConnectionData = GetPortConnectionDataFromSelf(nodeId, portId);
            return portConnectionData ?? GetPortConnectionDataFromOther(nodeId, portId);
        }

        public int GetPortConnectionDataCount()
        {
            return GetPortConnectionDataCountFromSelf() + GetPortConnectionDataFromOtherCount();
        }

        public PortConnectionData GetPortConnectionDataFromOther(int nodeId, int portId)
        {
            for (int i = 0; i < m_PortConnectionDataFromOther.Count; i++)
            {
                PortConnectionData portConnectionData = m_PortConnectionDataFromOther[i];
                if (portConnectionData.NodeID == nodeId && portConnectionData.PortID == portId)
                {
                    return portConnectionData;
                }
            }
            return null;
        }

        public int GetPortConnectionDataFromOtherCount()
        {
            return m_PortConnectionDataFromOther?.Count ?? 0;
        }

        public abstract PortConnectionData GetPortConnectionDataFromSelf(int nodeId, int portId);

        public abstract int GetPortConnectionDataCountFromSelf();

        public virtual void SetPortViewArgs(string name, Direction direction, Port.Capacity capacity, Color color)
        {
            m_PortViewArgs = new PortViewArgs(name, direction, capacity, color);
        }

        public virtual VisualElement CreatePortContentView(out PortView portView)
        {
            portView = new PortView(this);
            return portView;
        }

        public virtual bool CanConnect(BasePortData other)
        {
            return GetPortConnectionDataFromSelf(other.m_NodeData.NodeID, other.PortID) == null;
        }

        public virtual void Connect(BasePortData other)
        {
            other.m_PortConnectionDataFromOther.Add(new PortConnectionData
            {
                NodeID = m_NodeData.NodeID,
                PortID = PortID
            });
        }

        public virtual void Disconnect(BasePortData other)
        {
            for (int i = 0; i < other.m_PortConnectionDataFromOther.Count; i++)
            {
                PortConnectionData portConnectionData = other.m_PortConnectionDataFromOther[i];
                if (portConnectionData.NodeID == m_NodeData.NodeID && portConnectionData.PortID == PortID)
                {
                    other.m_PortConnectionDataFromOther.RemoveAt(i);
                    return;
                }
            }
        }

        public abstract BasePortData Clone();
#endif
    }
}