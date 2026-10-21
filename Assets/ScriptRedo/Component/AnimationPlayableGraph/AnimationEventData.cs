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

        /// <summary>
        /// 是否为全局事件。
        /// 用于查找动画事件函数时在局部查找还是全局查找
        /// </summary>
        [SerializeField] private bool m_IsStaticEvent;

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

        public bool IsStaticEvent()
        {
            return m_IsStaticEvent;
        }

        public IRuntimeData<IAnimationEventInvoker> GetEventArgs()
        {
            return m_EventArgs;
        }
    }

    //动画数据包括：首先属于哪一个Layer，其次属于Mixer下的哪一个Port，然后是AnimationClip和动画事件AnimationEventData[]集合
}