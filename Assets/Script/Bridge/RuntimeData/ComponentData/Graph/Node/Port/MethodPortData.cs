using System;
using System.Reflection;
using YBFramework.Component;
using YBFramework.EditorOnly;
#if UNITY_EDITOR
using UnityEditor.Experimental.GraphView;
using UnityEngine;
#endif

namespace YBFramework.Bridge
{
    [Serializable]
    public sealed class MethodPortData : BasePortData
    {
        private MethodInfo m_MethodInfo;

        public override BasePort CreateRuntimeInstance()
        {
            MethodPort methodPort = new();
            methodPort.InitializeFromData(this);
            return methodPort;
        }

        public MethodInfo GetMethodInfo()
        {
            return m_MethodInfo;
        }

        public void SetMethodInfo(MethodInfo methodInfo)
        {
            m_MethodInfo = methodInfo;
#if UNITY_EDITOR
            if (methodInfo != null)
            {
                m_ParameterInfos = methodInfo.GetParameters();
                m_ReturnType = methodInfo.ReturnType;
            }
#endif
        }
#if UNITY_EDITOR
        private Type m_ReturnType;

        private ParameterInfo[] m_ParameterInfos;

        public Type GetReturnType()
        {
            return m_ReturnType;
        }

        public ParameterInfo[] GetParameters()
        {
            return m_ParameterInfos;
        }

        public override bool CanConnect(BasePortData other)
        {
            return false;
        }

        public override PortConnectionData GetPortConnectionDataFromSelf(ushort nodeId, ushort portId)
        {
            return null;
        }

        public override int GetPortConnectionDataFromSelfCount()
        {
            return 0;
        }

        public override void SetPortViewArgs(string name, Direction direction, Port.Capacity capacity, Color color)
        {
            m_PortViewArgs = new PortViewArgs(name, direction, Port.Capacity.Multi, color);
        }
#endif
    }
}