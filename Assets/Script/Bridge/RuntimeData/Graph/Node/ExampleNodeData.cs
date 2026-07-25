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
        public override void CreateData()
        {
            StringInputTest = new ValueInputPortData<string>();
            StringInputTest.CreateData();
        }

        public override void Initialize()
        {
            base.Initialize();
            StringInputTest.SetFiledName(nameof(StringInputTest));
            StringInputTest.SetPortViewArgs("string输入测试", PortViewArgsTemplate.ValueInput);
        }
#endif
    }
}