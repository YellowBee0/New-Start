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
    public abstract class BasePortData
    {
        [SerializeField] protected int m_PortID;

        [SerializeField] protected bool m_IsUsed;

        public int GetPortID()
        {
            return m_PortID;
        }

        public bool GetIsUsed()
        {
            return m_IsUsed;
        }

        /// <summary>
        /// 获取端口中从自身发起的连线索引个数
        /// </summary>
        /// <returns>索引个数</returns>
        public abstract int GetIndexPortConnectionDataCount();

        /// <summary>
        /// 索引连线数据
        /// </summary>
        /// <param name="index">索引</param>
        /// <returns>连线数据，可能为null</returns>
        public abstract PortConnectionData IndexPortConnectionData(int index);

        public abstract BasePort CreateRuntimeInstance();
#if UNITY_EDITOR
        [SerializeField] private List<PortConnectionData> m_OtherPortConnectionsData;

        protected BaseNodeData m_NodeData;

        private Action<bool> m_OnIsUsedChanged;

        private string m_FieldName;

        public static TPortData CreatePortData<TPortData>() where TPortData : BasePortData, new()
        {
            TPortData portData = new();
            portData.InitializeSerializedData();
            return portData;
        }

        public IReadOnlyList<PortConnectionData> GetPortConnectionsData()
        {
            return m_OtherPortConnectionsData;
        }

        public BaseNodeData GetNodeData()
        {
            return m_NodeData;
        }

        public string GetFieldName()
        {
            return m_FieldName;
        }

        public void SetNodeData(BaseNodeData nodeData)
        {
            m_NodeData = nodeData;
        }

        public virtual void SetFieldName(string fieldName)
        {
            m_FieldName = fieldName;
        }

        public void SetPortID(int portID)
        {
            m_PortID = portID;
        }

        public void SetIsUsed(bool isUsed)
        {
            if (m_IsUsed != isUsed)
            {
                m_OnIsUsedChanged?.Invoke(isUsed);
                m_IsUsed = isUsed;
            }
        }

        public void RegisterOnIsUsedChanged(Action<bool> onIsUsedChanged)
        {
            if (onIsUsedChanged != null)
            {
                m_OnIsUsedChanged += onIsUsedChanged;
            }
        }

        public void UnregisterOnIsUsedChanged(Action<bool> onIsUsedChanged)
        {
            m_OnIsUsedChanged -= onIsUsedChanged;
        }

        #region Connection
        public int GetAllPortConnectionDataCount()
        {
            return GetSelfPortConnectionsDataCount() + GetOtherPortConnectionsDataCount();
        }

        /// <summary>
        /// 获取端口中从自身发起的有效的连线
        /// 与GetIndexPortConnectionDataCount的区别：
        /// GetIndexPortConnectionDataCount获取的是索引端口的总个数，比如存在一个PortConnectionData字段，就算这个字段为null或者NodeID和PortID都为0，也要计数
        /// GetSelfPortConnectionsDataCount获取的是有效连接，连接为null不算，NodeID和PortID为0也不算
        /// </summary>
        /// <returns>有效连接个数</returns>
        public virtual int GetSelfPortConnectionsDataCount()
        {
            return 0;
        }

        public int GetOtherPortConnectionsDataCount()
        {
            return m_OtherPortConnectionsData.Count;
        }

        public PortConnectionData FindAllPortConnectionData(int nodeID, int portID)
        {
            PortConnectionData portConnectionData = FindSelfPortConnectionData(nodeID, portID);
            return portConnectionData ?? FindOtherPortConnectionData(nodeID, portID);
        }

        public PortConnectionData FindSelfPortConnectionData(int nodeID, int portID)
        {
            int portConnectionDataCount = GetIndexPortConnectionDataCount();
            for (int i = 0; i < portConnectionDataCount; i++)
            {
                PortConnectionData portConnectionData = IndexPortConnectionData(i);
                if (portConnectionData != null && portConnectionData.NodeID == nodeID && portConnectionData.PortID == portID)
                {
                    return portConnectionData;
                }
            }
            return null;
        }

        public PortConnectionData FindOtherPortConnectionData(int nodeID, int portID)
        {
            for (int i = 0; i < m_OtherPortConnectionsData.Count; i++)
            {
                PortConnectionData portConnectionData = m_OtherPortConnectionsData[i];
                if (portConnectionData.NodeID == nodeID && portConnectionData.PortID == portID)
                {
                    return portConnectionData;
                }
            }
            return null;
        }

        public void DisconnectAll()
        {
            for (int i = GetIndexPortConnectionDataCount(); i >= 0; i--)
            {
                PortConnectionData portConnectionData = IndexPortConnectionData(i);
                if (portConnectionData == null || portConnectionData.NodeID == 0 || portConnectionData.PortID == 0)
                {
                    continue;
                }
                BaseNodeData nodeData = m_NodeData.GetGraphAsset().FindNodeData(portConnectionData.NodeID);
                BasePortData portData = nodeData.FindPortData(portConnectionData.PortID);
                Disconnect(portData);
            }
            for (int i = m_OtherPortConnectionsData.Count; i >= 0; i--)
            {
                PortConnectionData portConnectionData = m_OtherPortConnectionsData[i];
                BaseNodeData nodeData = m_NodeData.GetGraphAsset().FindNodeData(portConnectionData.PortID);
                BasePortData portData = nodeData.FindPortData(portConnectionData.PortID);
                portData.Disconnect(this);
            }
        }

        public virtual bool CanConnect(BasePortData other)
        {
            return FindAllPortConnectionData(other.m_NodeData.GetNodeID(), other.GetPortID()) == null;
        }

        public virtual void Connect(BasePortData other)
        {
            other.m_OtherPortConnectionsData.Add(new PortConnectionData
            {
                NodeID = m_NodeData.GetNodeID(),
                PortID = m_PortID
            });
        }

        public virtual void Disconnect(BasePortData other)
        {
            //被断开连接的端口需要同步断开自身来自其他端口的连接
            for (int i = 0; i < other.m_OtherPortConnectionsData.Count; i++)
            {
                PortConnectionData portConnectionData = other.m_OtherPortConnectionsData[i];
                if (portConnectionData.NodeID == m_NodeData.GetNodeID() && portConnectionData.PortID == m_PortID)
                {
                    other.m_OtherPortConnectionsData.RemoveAt(i);
                    return;
                }
            }
        }
        #endregion
        #region Data
        public abstract BasePortData AsTemplate();

        public virtual void CopyNonSerializedData(BasePortData templateData)
        {
            SetDirection(templateData.GetDirection());
            SetCapacity(templateData.GetCapacity());
            SetPortColor(templateData.GetPortColor());
        }

        public virtual void InitializeSerializedData()
        {
            m_OtherPortConnectionsData = new List<PortConnectionData>();
        }

        public virtual void MigrateSerializedData()
        {
        }
        #endregion
        #region Port view
        public abstract string GetPortName();

        public abstract Direction GetDirection();

        public abstract Port.Capacity GetCapacity();

        public abstract Color GetPortColor();

        public abstract void SetPortName(string portName);

        public abstract void SetDirection(Direction direction);

        public abstract void SetCapacity(Port.Capacity capacity);

        public abstract void SetPortColor(Color portColor);
        #endregion
#endif
    }
}