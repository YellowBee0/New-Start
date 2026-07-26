using System;
using System.Reflection;
using YBFramework.GameLogic.Graph;

namespace YBFramework.Bridge.Data
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

        public override BasePortData Clone()
        {
            return new MethodPortData
            {
                PortID = PortID
            };
        }

        public override PortConnectionData GetPortConnectionDataFromSelf(int nodeId, int portId)
        {
            return null;
        }

        public override int GetPortConnectionDataCountFromSelf()
        {
            return 0;
        }

        public override void MergeData(BasePortData dataToMerge)
        {
            base.MergeData(dataToMerge);
            SetMethodInfo(((MethodPortData)dataToMerge).m_MethodInfo);
        }
#endif
    }
}