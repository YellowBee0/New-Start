using System;
using System.Reflection;
using UnityEngine;
using YBFramework.GameLogic.Graph;
#if UNITY_EDITOR
using UnityEditor.Experimental.GraphView;
#endif

namespace YBFramework.Bridge.NewData
{
    [Serializable]
    public sealed class MethodPortData : BasePortData
    {
        private MethodInfo m_MethodInfo;

        private BasePortData[] m_UsedPortsData;

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

        public void SetUsedPortsData(params BasePortData[] usedPorts)
        {
            m_UsedPortsData = usedPorts;
        }

        public override bool CheckIsUsed()
        {
            bool isUsed = false;
            if (m_UsedPortsData != null)
            {
                for (int i = 0; i < m_UsedPortsData.Length; i++)
                {
                    if (m_UsedPortsData[i].CheckIsUsed())
                    {
                        isUsed = true;
                    }
                }
            }
            return isUsed;
        }

        public override int GetIndexPortConnectionDataCount()
        {
            return 0;
        }

        public override PortConnectionData IndexPortConnectionData(int index)
        {
            return null;
        }

        public override BasePort CreateRuntimeInstance()
        {
            //TODO:实现初始化逻辑
            return new MethodPort();
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

        private string m_PortName;

        private Direction m_Direction;

        private Port.Capacity m_Capacity;

        private Color m_PortColor;

        public override bool CanConnect(BasePortData other)
        {
            return false;
        }

        public override BasePortData AsTemplate()
        {
            MethodPortData templateData = CreatePortData<MethodPortData>();
            templateData.m_PortID = m_PortID;
            return templateData;
        }

        public override void CopyNonSerializedData(BasePortData templateData)
        {
            base.CopyNonSerializedData(templateData);
            MethodPortData data = (MethodPortData)templateData;
            m_MethodInfo = data.m_MethodInfo;
            m_ParameterInfos = data.m_ParameterInfos;
            m_ReturnType = data.m_ReturnType;
        }

        public override string GetPortName()
        {
            return m_PortName;
        }

        public override Direction GetDirection()
        {
            return m_Direction;
        }

        public override Port.Capacity GetCapacity()
        {
            return m_Capacity;
        }

        public override Color GetPortColor()
        {
            return m_PortColor;
        }

        public override void SetPortName(string portName)
        {
            m_PortName = portName;
        }

        public override void SetDirection(Direction direction)
        {
            m_Direction = direction;
        }

        public override void SetCapacity(Port.Capacity capacity)
        {
            m_Capacity = capacity;
        }

        public override void SetPortColor(Color portColor)
        {
            m_PortColor = portColor;
        }
#endif
    }
}