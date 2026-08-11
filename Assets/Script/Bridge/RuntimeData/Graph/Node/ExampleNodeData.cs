using System;
using YBFramework.GameLogic.Graph;
#if UNITY_EDITOR
using YBFramework.Bridge.Editor;
#endif

namespace YBFramework.Bridge.Data
{
    [Serializable]
#if UNITY_EDITOR
    [NodeMenu("示例", GraphType.Everything)]
#endif
    public sealed class ExampleNodeData : BaseNodeData
    {
        private MethodPortData m_MethodPortData;
        
        public ValueInputPortData<string> StringInputTest;

        public override BaseNode CreateRuntimeInstance()
        {
            throw new NotImplementedException();
        }

        public override bool Iterator(int index, out BasePortData current)
        {
            if (index == 0)
            {
                current = StringInputTest;
                return true;
            }
            current = null;
            return false;
        }
#if UNITY_EDITOR
        public override void InitializeSerializedData()
        {
            StringInputTest = CreatePortData<ValueInputPortData<string>>();
            StringInputTest.PortID = GetNextPortID();
        }

        protected override void OnInitialize()
        {
            StringInputTest.SetFiledName(nameof(StringInputTest));
            StringInputTest.SetPortViewArgs("string输入测试", PortViewArgsTemplate.ValueInput);
        }
#endif
    }
}