using System;
using UnityEngine;

namespace ScriptRedo
{
    [Serializable]
    public sealed class BehaviourComponentData : ISerializableData<BehaviourComponent>
    {
        [SerializeReference] private ISerializableData<IBehaviour> m_BehaviourData;

        public bool StartOnAddToEntityAndStopOnRemoveFromEntity = true;

        public ISerializableData<IBehaviour> GetBehaviourData()
        {
            return m_BehaviourData;
        }

        public BehaviourComponent CreateRuntimeInstance()
        {
            BehaviourComponent behaviourComponent = new();
            behaviourComponent.InitializeData(this);
            return behaviourComponent;
        }
    }
}