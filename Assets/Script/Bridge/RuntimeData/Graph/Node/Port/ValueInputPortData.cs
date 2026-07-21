using System;
using UnityEngine;
using YBFramework.GameLogic.Graph;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace YBFramework.Bridge.Data
{
    [Serializable]
    public sealed class ValueInputPortData<TValue> : BasePortData
    {
        public TValue Value;

        [SerializeField] private DelegatePortConnectionData m_DelegatePortConnectionData;

        public override BasePort CreateRuntimeInstance()
        {
            ValueInputPort<TValue> port = new();
            port.InitializeFromData(this);
            return port;
        }

        public override bool Iterator(int index, out PortConnectionData current)
        {
            if (index == 0)
            {
                current = m_DelegatePortConnectionData;
                return true;
            }
            current = null;
            return false;
        }
#if UNITY_EDITOR
        public override PortConnectionData GetPortConnectionDataFromSelf(int nodeId, int portId)
        {
            if (m_DelegatePortConnectionData.NodeID == nodeId && m_DelegatePortConnectionData.PortID == portId)
            {
                return m_DelegatePortConnectionData;
            }
            return null;
        }

        public override int GetPortConnectionDataCountFromSelf()
        {
            return m_DelegatePortConnectionData.NodeID == 0 && m_DelegatePortConnectionData.PortID == 0 ? 0 : 1;
        }

        public override BasePortData Clone()
        {
            ValueInputPortData<TValue> portData = new();
            string json = EditorJsonUtility.ToJson(this);
            EditorJsonUtility.FromJsonOverwrite(json, portData);
            portData.m_DelegatePortConnectionData = null;
            return portData;
        }
#endif
    }
}