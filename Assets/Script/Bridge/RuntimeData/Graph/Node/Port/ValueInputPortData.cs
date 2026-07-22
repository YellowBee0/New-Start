using System;
using UnityEngine;
using YBFramework.GameLogic.Graph;
#if UNITY_EDITOR
using UnityEditor.Experimental.GraphView;
using UnityEditor;
#endif

namespace YBFramework.Bridge.Data
{
    [Serializable]
    public sealed class ValueInputPortData<TValue> : BasePortData
    {
        public TValue Value;

        public DelegatePortConnectionData DelegatePortConnectionData;

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
                current = DelegatePortConnectionData;
                return true;
            }
            current = null;
            return false;
        }
#if UNITY_EDITOR
        public override void SetDirection(Direction direction)
        {
            if (direction == Direction.Output)
            {
                Debug.LogWarning("Value input port can only set input direction");
            }
            m_Direction = Direction.Input;
        }

        public override void SetCapacity(Port.Capacity capacity)
        {
            if (capacity == Port.Capacity.Multi)
            {
                Debug.LogWarning("Value input port can only set single capacity");
            }
            m_Capacity = Port.Capacity.Single;
        }

        public override PortConnectionData GetPortConnectionDataFromSelf(int nodeId, int portId)
        {
            if (DelegatePortConnectionData.NodeID == nodeId && DelegatePortConnectionData.PortID == portId)
            {
                return DelegatePortConnectionData;
            }
            return null;
        }

        public override int GetPortConnectionDataCountFromSelf()
        {
            return DelegatePortConnectionData.NodeID == 0 && DelegatePortConnectionData.PortID == 0 ? 0 : 1;
        }

        public override BasePortData Clone()
        {
            ValueInputPortData<TValue> portData = new();
            string json = EditorJsonUtility.ToJson(this);
            EditorJsonUtility.FromJsonOverwrite(json, portData);
            portData.DelegatePortConnectionData.NodeID = 0;
            portData.DelegatePortConnectionData.PortID = 0;
            return portData;
        }
#endif
    }
}