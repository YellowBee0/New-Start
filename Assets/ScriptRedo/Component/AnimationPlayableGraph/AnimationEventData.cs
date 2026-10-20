using System;
using UnityEngine;

namespace ScriptRedo
{
    [Serializable]
    public sealed class AnimationEventData
    {
        [SerializeField] private string m_EventProviderName;

        [SerializeField] private string m_EventName;

        [SerializeField] private float m_TriggerTime;

        [SerializeReference] private IRuntimeData<IAnimationEventInvoker> m_EventArgs;

        public string GetEventProviderName()
        {
            return m_EventProviderName;
        }

        public string GetEventName()
        {
            return m_EventName;
        }

        public float GetTriggerTime()
        {
            return m_TriggerTime;
        }

        public IRuntimeData<IAnimationEventInvoker> GetEventArgs()
        {
            return m_EventArgs;
        }
    }
}