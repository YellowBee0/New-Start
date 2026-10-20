#if UNITY_EDITOR
using System;

namespace ScriptRedo
{
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class AnimationEventAttribute : Attribute
    {
        public readonly string EventName;

        public AnimationEventAttribute(string eventName)
        {
            EventName = eventName;
        }
    }
}
#endif