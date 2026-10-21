#if UNITY_EDITOR
using System;

namespace ScriptRedo
{
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class InstanceAnimationEventAttribute : Attribute
    {
        public readonly string EventName;

        public InstanceAnimationEventAttribute(string eventName)
        {
            EventName = eventName;
        }
    }
}
#endif