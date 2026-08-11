using System;
using YBFramework.Common;
using YBFramework.GameLogic.Graph;
#if UNITY_EDITOR
using YBFramework.Bridge.Editor;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
#endif

namespace YBFramework.Bridge.Data
{
    [Serializable]
    public abstract class BasePortData : IValueIterator<PortConnectionData>
    {
        public int PortID;

        /// <summary>
        /// 端口是否存在连接。
        /// 在运行是通过这个字段判断这个是否需要创建这个端口数据的运行实例。
        /// true需要创建，false反之。
        /// 如果要判断一个节点是否有用，不能通过判断节点下所有端口是否存在IsUsed为true的判断为被引用。
        /// 因为IsUsed对只输入端口一般都是为true的，就算没连接其他端口，自身节点都有可能直接使用值；
        /// 还有可能两个或者多个节点的端口之间是循环引用关系，这些节点都是没用的。
        /// 如果要判断的话还是得找到每个节点连接端口，并保存节点id到集合中，然后去重遍历集合查看是否每个节点都在集合里。
        /// </summary>
        public bool IsUsed;

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

        public ValueEnumerator<PortConnectionData> GetEnumerator()
        {
            throw new NotImplementedException();
        }
#if UNITY_EDITOR
        [SerializeField] protected List<PortConnectionData> m_PortConnectionsDataFromOther;

        protected BaseNodeData m_NodeData;

        protected string m_FiledName;

        protected string m_PortName;

        protected Direction m_Direction;

        protected Port.Capacity m_Capacity;

        protected Color m_PortColor;

        public IReadOnlyList<PortConnectionData> GetPortConnectionsDataFromOther()
        {
            return m_PortConnectionsDataFromOther;
        }

        public BaseNodeData GetNodeData()
        {
            return m_NodeData;
        }

        public string GetFiledName()
        {
            return m_FiledName;
        }

        public string GetPortName()
        {
            return m_PortName;
        }

        public Direction GetDirection()
        {
            return m_Direction;
        }

        public Port.Capacity GetCapacity()
        {
            return m_Capacity;
        }

        public Color GetPortColor()
        {
            return m_PortColor;
        }

        public void SetPortViewArgs(string portName, PortViewArgsTemplate argsTemplate)
        {
            SetPortName(portName);
            switch (argsTemplate)
            {
                case PortViewArgsTemplate.Default:
                    SetDirection(default);
                    SetCapacity(default);
                    SetPortColor(default);
                    return;
                case PortViewArgsTemplate.LogicInput:
                    SetDirection(Direction.Input);
                    SetCapacity(Port.Capacity.Multi);
                    SetPortColor(Color.red);
                    return;
                case PortViewArgsTemplate.LogicOutput:
                    SetDirection(Direction.Output);
                    SetCapacity(Port.Capacity.Multi);
                    SetPortColor(Color.red);
                    return;
                case PortViewArgsTemplate.ValueInput:
                    SetDirection(Direction.Input);
                    SetCapacity(Port.Capacity.Single);
                    SetPortColor(Color.blue);
                    return;
                case PortViewArgsTemplate.ValueOutput:
                    SetDirection(Direction.Output);
                    SetCapacity(Port.Capacity.Multi);
                    SetPortColor(Color.blue);
                    return;
                default:
                    throw new ArgumentOutOfRangeException(nameof(argsTemplate), argsTemplate, null);
            }
        }

        public virtual void SetNodeData(BaseNodeData nodeData)
        {
            m_NodeData = nodeData;
        }

        public virtual void SetFiledName(string filedName)
        {
            m_FiledName = filedName;
        }

        public virtual void SetPortName(string portName)
        {
            m_PortName = portName;
        }

        public virtual void SetDirection(Direction direction)
        {
            m_Direction = direction;
        }

        public virtual void SetCapacity(Port.Capacity capacity)
        {
            m_Capacity = capacity;
        }

        public virtual void SetPortColor(Color portColor)
        {
            m_PortColor = portColor;
        }

        public virtual bool CanConnect(BasePortData other)
        {
            return GetPortConnectionData(other.m_NodeData.NodeID, other.PortID) == null;
        }

        public virtual void Connect(BasePortData other)
        {
            //被连接的端口需要设置IsUsed为true，且来自其他端口连接需要添加当前端口
            IsUsed = true;
            other.IsUsed = true;
            other.m_PortConnectionsDataFromOther.Add(new PortConnectionData
            {
                NodeID = m_NodeData.NodeID,
                PortID = PortID
            });
        }

        public virtual void Disconnect(BasePortData other)
        {
            //被断开连接的端口需要同步断开自身来自其他端口的连接
            for (int i = 0; i < other.m_PortConnectionsDataFromOther.Count; i++)
            {
                PortConnectionData portConnectionData = other.m_PortConnectionsDataFromOther[i];
                if (portConnectionData.NodeID == m_NodeData.NodeID && portConnectionData.PortID == PortID)
                {
                    other.m_PortConnectionsDataFromOther.RemoveAt(i);
                    if (other.GetPortConnectionDataCount() == 0)
                    {
                        other.IsUsed = false;
                    }
                    return;
                }
            }
        }

        public void DisconnectAll()
        {
            foreach (PortConnectionData portConnectionData in (IValueIterator<PortConnectionData>)this)
            {
                if (portConnectionData.NodeID == 0 || portConnectionData.PortID == 0)
                {
                    continue;
                }
                BaseNodeData nodeData = m_NodeData.GetGraphAsset().GetNodeData(portConnectionData.NodeID);
                BasePortData portData = nodeData.GetPortData(portConnectionData.PortID);
                Disconnect(portData);
            }
            for (int i = 0; i < m_PortConnectionsDataFromOther.Count; i++)
            {
                PortConnectionData portConnectionData = m_PortConnectionsDataFromOther[i];
                BaseNodeData nodeData = m_NodeData.GetGraphAsset().GetNodeData(portConnectionData.PortID);
                BasePortData portData = nodeData.GetPortData(portConnectionData.PortID);
                portData.Disconnect(this);
            }
        }

        public PortConnectionData GetPortConnectionData(int nodeId, int portId)
        {
            PortConnectionData portConnectionData = GetPortConnectionDataFromSelf(nodeId, portId);
            return portConnectionData ?? GetPortConnectionDataFromOther(nodeId, portId);
        }

        public PortConnectionData GetPortConnectionDataFromOther(int nodeId, int portId)
        {
            for (int i = 0; i < m_PortConnectionsDataFromOther.Count; i++)
            {
                PortConnectionData portConnectionData = m_PortConnectionsDataFromOther[i];
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
            return m_PortConnectionsDataFromOther?.Count ?? 0;
        }

        public abstract int GetPortConnectionDataCountFromSelf();

        public virtual void InitializeSerializedData()
        {
            m_PortConnectionsDataFromOther = new List<PortConnectionData>();
        }

        public abstract BasePortData Clone();

        /// <summary>
        /// 用于代理端口从真实的端口获取不可序列化的数据，比如绘制参数PortViewArgs，MethodPortData的MethodInfo
        /// 调用MergeData必须保证端口对应
        /// </summary>
        /// <param name="dataToMerge">代理目标</param>
        public virtual void MergeData(BasePortData dataToMerge)
        {
            m_Direction = dataToMerge.GetDirection();
            m_Capacity = dataToMerge.GetCapacity();
            m_PortColor = dataToMerge.GetPortColor();
        }

        /// <summary>
        /// 迁移端口数据
        /// 如果端口新增/删除需要序列化且带有初始值的数据时需要在蓝图里调用这个函数
        /// 函数在调用过一次后内部实现需要及时删除，以免出现重复迁移数据
        /// </summary>
        /// <param name="graphAsset">端口数据所在的GraphAsset，用于保存迁移的数据以便显示和查看</param>
        /// <returns>是否迁移</returns>
        public virtual bool MigrateSerializedData(GraphAsset graphAsset)
        {
            return false;
        }
#endif
    }
}