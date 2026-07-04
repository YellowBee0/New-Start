using System;
using UnityEngine;
using YBFramework.Component;

namespace YBFramework.Bridge
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
#if UNITY_EDITOR
        public override PortConnectionData GetPortConnectionDataFromSelf(ushort nodeId, ushort portId)
        {
            //TODO:m_DelegatePortConnectionData始终会存在值，，默认为0，0，需要采用其他的判断方式
            return m_DelegatePortConnectionData;
        }

        public override int GetPortConnectionDataFromSelfCount()
        {
            //TODO:m_DelegatePortConnectionData始终会存在值，，默认为0，0，需要采用其他的判断方式
            return m_DelegatePortConnectionData != null ? 1 : 0;
        }
#endif
    }
}