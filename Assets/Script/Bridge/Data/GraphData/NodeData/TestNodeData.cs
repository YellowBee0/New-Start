#if UNITY_EDITOR
using System;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using YBFramework.Bridge.Editor;
using YBFramework.GameLogic.Graph;

namespace YBFramework.Bridge.Data
{
    [Serializable]
    [NodeMenu("测试", GraphType.Everything)]
    public sealed class TestNodeData : BaseNodeData
    {
        [SerializeField] private ActionPortData m_InvokePortData;

        [SerializeField] private ValueInputPortData<int> m_IntInputPort;

        public override int GetPortsDataCount()
        {
            return 2;
        }

        public override BasePortData PortDataOfIndex(int index)
        {
            switch (index)
            {
                case 0:
                    return m_InvokePortData;
                case 1:
                    return m_IntInputPort;
                default:
                    return null;
            }
        }

        public override BaseNode CreateRuntimeInstance(NodeSliceData nodeSliceData)
        {
            throw new NotImplementedException();
        }

        public override void DFSExecutionFlow(DFSGraphAsset dfsGraphAsset, BasePortData portData)
        {
            throw new NotImplementedException();
        }

        public override void InitializeSerializedData()
        {
            m_InvokePortData = new ActionPortData();
            m_InvokePortData.SetPortID(1);
            m_InvokePortData.InitializeSerializedData();
            m_IntInputPort = new ValueInputPortData<int>();
            m_IntInputPort.SetPortID(2);
            m_IntInputPort.InitializeSerializedData();
        }

        protected override void OnInitializePortData()
        {
            m_InvokePortData.SetPortName("调用输出");
            m_InvokePortData.SetDirection(Direction.Output);
            m_InvokePortData.SetPortColor(Color.cyan);
            m_IntInputPort.SetPortName("int输入");
            m_IntInputPort.SetPortColor(Color.yellow);
            m_IntInputPort.SetFieldPath(nameof(m_IntInputPort));
        }
    }
}
#endif