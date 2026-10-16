using System;
using UnityEngine;

namespace ScriptRedo
{
    [Serializable]
    public sealed class BuffBehaviourComponentData : ISerializableData<BuffBehaviourComponent>
    {
        public BuffType BuffType;

        [SerializeReference] public ISerializableData<IBuffBehaviour> BuffBehaviourData;

        //如何处理重复添加

        public BuffBehaviourComponent CreateRuntimeInstance()
        {
            BuffBehaviourComponent buffBehaviourComponent = new BuffBehaviourComponent();
            buffBehaviourComponent.InitializeData(this);
            return buffBehaviourComponent;
        }
    }
}