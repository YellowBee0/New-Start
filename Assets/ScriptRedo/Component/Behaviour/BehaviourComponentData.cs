using System;
using UnityEngine;

namespace ScriptRedo
{
    [Serializable]
    public sealed class BehaviourComponentData : IComponentData
    {
        [SerializeReference] private IBehaviourData m_BehaviourData;

        public IBehaviourData GetBehaviourData()
        {
            return m_BehaviourData;
        }

        public IComponent CreateRuntimeComponent()
        {
            BehaviourComponent behaviourComponent = new();
            behaviourComponent.InitializeData(this);
            return behaviourComponent;
        }
    }
}