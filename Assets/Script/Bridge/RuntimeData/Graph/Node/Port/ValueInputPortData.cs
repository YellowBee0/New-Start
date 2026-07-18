using System;
using UnityEditor;
using UnityEngine;
using YBFramework.GameLogic.Graph;

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
            //TODO:m_DelegatePortConnectionData始终会存在值，，默认为0，0，需要采用其他的判断方式
            return m_DelegatePortConnectionData;
        }

        public override int GetPortConnectionDataCountFromSelf()
        {
            //TODO:m_DelegatePortConnectionData始终会存在值，，默认为0，0，需要采用其他的判断方式
            return m_DelegatePortConnectionData != null ? 1 : 0;
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