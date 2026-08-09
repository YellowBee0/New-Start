using System;

namespace Script.Common
{
    [AttributeUsage(AttributeTargets.Method, Inherited = false)]
    public sealed class MethodMarkAttribute : Attribute
    {
        public readonly string MarkName;

        public MethodMarkAttribute(string markName)
        {
            MarkName = markName;
        }
    }
}