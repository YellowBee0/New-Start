using System;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using YBFramework.Bridge.Editor;
using YBFramework.GameLogic.Graph;

namespace YBFramework.Bridge.Data
{
#if UNITY_EDITOR
    [NodeMenu("通用/数据", GraphType.Everything)]
#endif
    [Serializable]
    public sealed class ValueNodeData<TValue> : BaseNodeData
    {
        [SerializeField] private TValue m_Value;

        private MethodPortData m_ReturnValue;

        private MethodPortData m_EnterSetValue;

        private ValueInputPortData<TValue> m_SetValueParameter;

        public override int GetPortsDataCount()
        {
            return 3;
        }

        public override BasePortData PortDataOfIndex(int index)
        {
            switch (index)
            {
                case 0:
                    return m_ReturnValue;
                case 1:
                    return m_EnterSetValue;
                case 2:
                    return m_SetValueParameter;
                default:
                    return null;
            }
        }

        public override BaseNode CreateRuntimeNode(NodeCheckResult nodeCheckResult, BuildNodeData buildNodeData)
        {
            throw new NotImplementedException();
        }

        public override void CheckExecutionFlow(GraphCheckContext graphCheckContext, int portID)
        {
            throw new NotImplementedException();
        }

        public override void InitializeSerializedData()
        {
            m_ReturnValue = new MethodPortData();
            m_EnterSetValue = new MethodPortData();
            m_SetValueParameter = new ValueInputPortData<TValue>();
        }

        protected override void OnInitializePortData()
        {
            m_ReturnValue.SetMethodInfo(ValueNode<TValue>.GetValueMethod);
            m_EnterSetValue.SetMethodInfo(ValueNode<TValue>.SetValueMethod);
            m_ReturnValue.SetPortViewData("获取数据", Direction.Output, Port.Capacity.Multi, Color.cyan);
            m_EnterSetValue.SetPortViewData("设置数据", Direction.Input, Port.Capacity.Multi, Color.red);
            m_SetValueParameter.SetPortName("设置的数据");
            m_SetValueParameter.SetPortColor(Color.cyan);
        }
    }
}