using System;
using UnityEngine;

namespace ScriptRedo
{
    [Serializable]
    public sealed class BehaviourComponentData : IRuntimeData<BehaviourComponent>
    {
        [SerializeReference] private IRuntimeData<IBehaviour> m_BehaviourData;

        public bool StartOnAddToEntityAndStopOnRemoveFromEntity = true;

        public IRuntimeData<IBehaviour> GetBehaviourData()
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