using System;
using YBFramework.Common;
using YBFramework.GameLogic.Graph;
#if UNITY_EDITOR
using YBFramework.Bridge.Editor;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
#endif

namespace YBFramework.Bridge.Data
{
    [Serializable]
    public abstract class BasePortData : IValueIterator<PortConnectionData>
    {
        public int PortID;

        public abstract BasePort CreateRuntimeInstance();

        /// <summary>
        /// 获取端口中所有自身的连接（编辑器中连接的时候主动调用Connect时连接的连线）
        /// </summary>
        /// <param name="index">索引</param>
        /// <param name="current">当前数据</param>
        /// <returns>是否执行到下一个元素</returns>
        public virtual bool Iterator(int index, out PortConnectionData current)
        {
            current = null;
            return false;
        }
#if UNITY_EDITOR
        [SerializeField] private List<PortConnectionData> m_PortConnectionDataFromOther;

        protected BaseNodeData m_NodeData;

        protected PortViewArgs m_PortViewArgs;

        public void SetNodeData(BaseNodeData nodeData)
        {
            m_NodeData = nodeData;
        }
        
        public virtual PortViewArgs GetPortViewArgs()
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

        public virtual void SetPortViewArgs(string name, Direction direction, Port.Capacity capacity, Color color)
        {
            m_PortViewArgs = new PortViewArgs(name, direction, capacity, color);
        }

        public virtual VisualElement CreatePortContentView(out PortView portView)
        {
            portView = new PortView(this, m_PortViewArgs.Name, m_PortViewArgs.Direction, m_PortViewArgs.Capacity, m_PortViewArgs.Color);
            return portView;
        }

        public virtual bool CanConnect(BasePortData other)
        {
            return GetPortConnectionData(other.m_NodeData.NodeID, other.PortID) == null;
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

        public PortConnectionData GetPortConnectionData(int nodeId, int portId)
        {
            PortConnectionData portConnectionData = GetPortConnectionDataFromSelf(nodeId, portId);
            return portConnectionData ?? GetPortConnectionDataFromOther(nodeId, portId);
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

        public abstract PortConnectionData GetPortConnectionDataFromSelf(int nodeId, int portId);

        public int GetPortConnectionDataCount()
        {
            return GetPortConnectionDataCountFromSelf() + GetPortConnectionDataFromOtherCount();
        }

        public int GetPortConnectionDataFromOtherCount()
        {
            return m_PortConnectionDataFromOther?.Count ?? 0;
        }

        public abstract int GetPortConnectionDataCountFromSelf();

        public abstract BasePortData Clone();
#endif
    }
}