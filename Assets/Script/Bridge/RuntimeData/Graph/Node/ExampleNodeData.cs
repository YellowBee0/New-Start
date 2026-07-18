using System;
using UnityEngine;
using YBFramework.Bridge.Editor;
using YBFramework.GameLogic.Graph;

namespace YBFramework.Bridge.Data
{
    [Serializable]
    public sealed class ExampleNodeData : BaseNodeData
    {
        [SerializeField] private ValueInputPortData<string> m_StringInputTest;

        public override BaseNode CreateRuntimeInstance()
        {
            throw new NotImplementedException();
        }

        public override bool Iterator(int index, out BasePortData current)
        {
            if (index == 0)
            {
                current = m_StringInputTest;
                return true;
            }
            current = null;
            return false;
        }
#if UNITY_EDITOR
        public override void Initialize()
        {
            m_StringInputTest.SetPortViewArgs("string输入测试", PortViewArgsTemplate.ValueInput);
        }
#endif
    }
}