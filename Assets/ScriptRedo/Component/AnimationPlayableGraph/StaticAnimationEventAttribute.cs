#if UNITY_EDITOR
using System;

namespace ScriptRedo
{
    [AttributeUsage(AttributeTargets.Method, Inherited = false)]
    public sealed class StaticAnimationEventAttribute : Attribute
    {
        public readonly string Name;

        public readonly string GroupName;

        public StaticAnimationEventAttribute(string name, string groupName)
        {
            Name = name;
            GroupName = groupName;
        }
    }
}
#endif