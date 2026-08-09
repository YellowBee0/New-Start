#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEngine;
using YBFramework.Bridge.Editor;
using YBFramework.GameLogic.Graph;

namespace YBFramework.Bridge.Data
{
    [Serializable]
    public sealed class ProxyHelperPortData : BasePortData
    {
        private static readonly List<(ProxyHelperPortData, int)> s_ConnectDataToSave = new();

        private static readonly List<(ProxyHelperPortData, int)> s_DisconnectDataToSave = new();

        public static IReadOnlyList<(ProxyHelperPortData, int)> GetConnectDataToSave()
        {
            return s_ConnectDataToSave;
        }

        public static IReadOnlyList<(ProxyHelperPortData, int)> GetDisconnectDataToSave()
        {
            return s_DisconnectDataToSave;
        }

        public static void ClearDataToSave()
        {
            s_ConnectDataToSave.Clear();
            s_DisconnectDataToSave.Clear();
        }

        public string ProxyName;

        /// <summary>
        /// 代理端口的索引，同时也是连线数据
        /// </summary>
        [SerializeField] private PortConnectionData m_ProxyPortIndex;

        private BasePortData m_TargetPortData;

        public PortConnectionData GetProxyPortIndex()
        {
            return m_ProxyPortIndex;
        }

        public BasePortData GetProxyPortData()
        {
            return m_TargetPortData;
        }

        public void SetTargetPortData(BasePortData targetPortData)
        {
            m_TargetPortData = targetPortData;
        }

        public override BasePort CreateRuntimeInstance()
        {
            Debug.Log("Editor only port:proxy helper port is tried to create a runtime port");
            return null;
        }

        public override bool Iterator(int index, out PortConnectionData current)
        {
            if (index == 0)
            {
                current = m_ProxyPortIndex;
                return true;
            }
            current = null;
            return false;
        }

        public override PortConnectionData GetPortConnectionDataFromSelf(int nodeId, int portId)
        {
            if (m_ProxyPortIndex.NodeID == nodeId && m_ProxyPortIndex.PortID == portId)
            {
                return m_ProxyPortIndex;
            }
            return null;
        }

        public override int GetPortConnectionDataCountFromSelf()
        {
            if (m_ProxyPortIndex.NodeID != 0 && m_ProxyPortIndex.PortID != 0)
            {
                return 1;
            }
            return 0;
        }

        public override bool CanConnect(BasePortData other)
        {
            return base.CanConnect(other) && other is not ProxyHelperPortData;
        }

        public override void Connect(BasePortData other)
        {
            base.Connect(other);
            m_ProxyPortIndex.NodeID = other.GetNodeData().NodeID;
            m_ProxyPortIndex.PortID = other.PortID;
            bool isDisconnect = false;
            for (int i = 0; i < s_DisconnectDataToSave.Count; i++)
            {
                (ProxyHelperPortData, int) disconnectDataToSave = s_DisconnectDataToSave[i];
                if (disconnectDataToSave.Item1 == this && disconnectDataToSave.Item2 == other.PortID)
                {
                    isDisconnect = true;
                    s_DisconnectDataToSave.RemoveAt(i);
                    if (s_DisconnectDataToSave.Count == 0)
                    {
                        GraphDataSaveProcessBridge.UnregisterProcess("Save Disconnect Proxy Helper Node Data");
                    }
                    break;
                }
            }
            if (!isDisconnect)
            {
                if (s_ConnectDataToSave.Count == 0)
                {
                    GraphDataSaveProcessBridge.RegisterProcess("Save Connect Proxy Helper Node Data");
                }
                s_ConnectDataToSave.Add((this, other.PortID));
            }
        }

        public override void Disconnect(BasePortData other)
        {
            base.Disconnect(other);
            if (m_ProxyPortIndex.NodeID == other.GetNodeData().NodeID && m_ProxyPortIndex.PortID == other.PortID)
            {
                m_ProxyPortIndex.NodeID = 0;
                m_ProxyPortIndex.PortID = 0;
                bool isConnect = false;
                for (int i = 0; i < s_ConnectDataToSave.Count; i++)
                {
                    (ProxyHelperPortData, int) connectDataToSave = s_ConnectDataToSave[i];
                    if (connectDataToSave.Item1 == this && connectDataToSave.Item2 == other.PortID)
                    {
                        isConnect = true;
                        s_ConnectDataToSave.RemoveAt(i);
                        if (s_ConnectDataToSave.Count == 0)
                        {
                            GraphDataSaveProcessBridge.UnregisterProcess("Save Connect Proxy Helper Node Data");
                        }
                        break;
                    }
                }
                if (!isConnect)
                {
                    if (s_DisconnectDataToSave.Count == 0)
                    {
                        GraphDataSaveProcessBridge.RegisterProcess("Save Disconnect Proxy Helper Node Data");
                    }
                    s_DisconnectDataToSave.Add((this, other.PortID));
                }
            }
            if (GetPortConnectionDataCount() == 0)
            {
                IsUsed = false;
            }
        }

        public override BasePortData Clone()
        {
            throw new Exception("this port can not clone for proxy port");
        }
    }
}
#endif