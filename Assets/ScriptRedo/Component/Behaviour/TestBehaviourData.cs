using System;

namespace ScriptRedo
{
    [Serializable]
    public sealed class TestBehaviourData : ISerializableData<TestBehaviour>
    {
        public TestBehaviour CreateRuntimeInstance()
        {
            TestBehaviour behaviour = new();
            behaviour.InitializeData(this);
            return behaviour;
        }
    }
}