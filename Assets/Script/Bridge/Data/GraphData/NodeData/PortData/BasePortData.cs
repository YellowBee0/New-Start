using System;
using UnityEngine;
using YBFramework.GameLogic.Graph;
#if UNITY_EDITOR
using UnityEditor.Experimental.GraphView;
#endif

namespace YBFramework.Bridge.Data
{
    [Serializable]
    public abstract class BasePortData
    {
        public abstract int GetPortID();

        public abstract bool HasSubPortData();

        /*/// <summary>
        /// 是否被作为子图的端口，用于在节点的可执行检查进入子图，子图端口每检查到自己可执行就需要判断自己是否被作为了子图的端口，是就需要返回父图执行父图的检查
        /// </summary>
        /// <returns>true是，false反之</returns>
        public abstract bool IsSubPort();*/

        public abstract int GetPortConnectionsDataCount();

        public abstract PortConnectionData PortConnectionDataOfIndex(int index);

        public abstract BasePort CreateRuntimeInstance();

        public void CheckExecutionFlow(CheckGraphExecutionContext checkGraphExecutionContext)
        {
            int portConnectionsDataCount = GetPortConnectionsDataCount();
            for (int i = 0; i < portConnectionsDataCount; i++)
            {
                PortConnectionData portConnectionData = PortConnectionDataOfIndex(i);
                if (portConnectionData.IsValid())
                {
                    BaseNodeData nodeData = checkGraphExecutionContext.GraphAsset.FindNodeData(portConnectionData.NodeID);
                    if (nodeData != null)
                    {
                        BasePortData portData = nodeData.FindPortData(portConnectionData.PortID);
                        if (portData != null)
                        {
                            nodeData.CheckExecutionFlow(checkGraphExecutionContext, portData);
                        }
                    }
                }
            }
            if (HasSubPortData())
            {
                CheckGraphExecutionContext parent = checkGraphExecutionContext.Parent;
                if (parent != null)
                {
                    SubNodeData subNodeData = (SubNodeData)parent.CurrentCheckNodeExecutionContext.NodeData;
                    BasePortData portData = subNodeData.FindSubPortDataBySubPortAddress(checkGraphExecutionContext.CurrentCheckNodeExecutionContext.NodeData.GetNodeID(), GetPortID());
                    if (portData != null)
                    {
                        subNodeData.CheckExecutionFlow(parent, portData);
                    }
                }
            }
        }
#if UNITY_EDITOR

        #region Base data

        public abstract BaseNodeData GetNodeData();

        public abstract void SetPortID(int portID);

        public abstract void SetHasSubPortData(bool hasSubPortData);

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
                if (portConnectionData.NodeID == nodeID && portConnectionData.PortID == portID)
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
                if (portConnectionData.NodeID == nodeID && portConnectionData.PortID == portID)
                {
                    return portConnectionData;
                }
            }
            return null;
        }

        public void DisconnectAll()
        {
            int count = GetPortConnectionsDataCount();
            for (int i = count - 1; i >= 0; i--)
            {
                PortConnectionData portConnectionData = PortConnectionDataOfIndex(i);
                if (portConnectionData.IsValid())
                {
                    BaseNodeData nodeData = GetNodeData().GetGraphAsset().FindNodeData(portConnectionData.NodeID);
                    BasePortData portData = nodeData.FindPortData(portConnectionData.PortID);
                    Disconnect(portData);
                }
            }
            count = GetOtherPortConnectionsDataCount();
            for (int i = count - 1; i >= 0; i--)
            {
                PortConnectionData portConnectionData = OtherPortConnectDataOfIndex(i);
                if (portConnectionData != null)
                {
                    BaseNodeData nodeData = GetNodeData().GetGraphAsset().FindNodeData(portConnectionData.NodeID);
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