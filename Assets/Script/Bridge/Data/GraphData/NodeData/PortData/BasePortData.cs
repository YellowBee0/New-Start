using System;
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
        public abstract int GetPortID();

        public abstract bool HasSubPortData();

        public abstract int GetPortConnectionsDataCount();

        public abstract PortConnectionData PortConnectionDataOfIndex(int index);

        public abstract BasePort CreateRuntimeInstance();
#if UNITY_EDITOR
        #region Base data
        private string m_FieldName;

        public string GetFieldName()
        {
            return m_FieldName;
        }

        public abstract BaseNodeData GetNodeData();

        public abstract void SetPortID(int portID);

        public abstract void SetHasSubPortData(bool hasSubPortData);

        public virtual void SetFieldName(string fieldName)
        {
            m_FieldName = fieldName;
        }

        public abstract void SetNodeData(BaseNodeData nodeData);
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

        #region Data
        public abstract void InitializeSerializedData();

        /// <summary>
        /// 创建当前端口的子端口
        /// </summary>
        /// <returns>当前端口的部分clone版本</returns>
        public abstract BasePortData CreateSubPortData();

        public virtual void RevertNonSerializedData(BasePortData subSourcePortData)
        {
            SetDirection(subSourcePortData.GetDirection());
            SetCapacity(subSourcePortData.GetCapacity());
            SetPortColor(subSourcePortData.GetPortColor());
        }
        #endregion

        #region Connection
        public int GetAllPortConnectionDataCount()
        {
            return GetPortConnectionsDataCount() + GetOtherPortConnectionsDataCount();
        }

        public PortConnectionData FindAllPortConnectionData(int nodeID, int portID)
        {
            PortConnectionData portConnectionData = FindSelfPortConnectionData(nodeID, portID);
            return portConnectionData ?? FindOtherPortConnectionData(nodeID, portID);
        }

        public PortConnectionData FindSelfPortConnectionData(int nodeID, int portID)
        {
            int portConnectionDataCount = GetPortConnectionsDataCount();
            for (int i = 0; i < portConnectionDataCount; i++)
            {
                PortConnectionData portConnectionData = PortConnectionDataOfIndex(i);
                if (portConnectionData != null && portConnectionData.NodeID == nodeID && portConnectionData.PortID == portID)
                {
                    return portConnectionData;
                }
            }
            return null;
        }

        public PortConnectionData FindOtherPortConnectionData(int nodeID, int portID)
        {
            int otherPortConnectDataCount = GetOtherPortConnectionsDataCount();
            for (int i = 0; i < otherPortConnectDataCount; i++)
            {
                PortConnectionData portConnectionData = OtherPortConnectDataOfIndex(i);
                if (portConnectionData != null && portConnectionData.NodeID == nodeID && portConnectionData.PortID == portID)
                {
                    return portConnectionData;
                }
            }
            return null;
        }

        public void DisconnectAll()
        {
            int count = GetPortConnectionsDataCount();
            for (int i = count; i >= 0; i--)
            {
                PortConnectionData portConnectionData = PortConnectionDataOfIndex(i);
                if (portConnectionData == null || portConnectionData.NodeID == 0 || portConnectionData.PortID == 0)
                {
                    continue;
                }
                BaseNodeData nodeData = GetNodeData().GetGraphAsset().FindNodeData(portConnectionData.NodeID);
                BasePortData portData = nodeData.FindPortData(portConnectionData.PortID);
                Disconnect(portData);
            }
            count = GetOtherPortConnectionsDataCount();
            for (int i = count; i >= 0; i--)
            {
                PortConnectionData portConnectionData = OtherPortConnectDataOfIndex(i);
                if (portConnectionData != null)
                {
                    BaseNodeData nodeData = GetNodeData().GetGraphAsset().FindNodeData(portConnectionData.PortID);
                    BasePortData portData = nodeData.FindPortData(portConnectionData.PortID);
                    portData.Disconnect(this);
                }
            }
        }

        public abstract int GetOtherPortConnectionsDataCount();

        public abstract PortConnectionData OtherPortConnectDataOfIndex(int index);

        public virtual bool CanConnect(BasePortData other)
        {
            return FindAllPortConnectionData(other.GetNodeData().GetNodeID(), other.GetPortID()) == null;
        }

        public abstract void Connect(BasePortData other);

        public abstract void Disconnect(BasePortData other);

        public abstract void BeConnected(BasePortData other);

        public abstract void BeDisconnected(BasePortData other);
        #endregion
#endif
    }
}