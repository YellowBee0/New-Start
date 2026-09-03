using System;
using JetBrains.Annotations;

namespace Script.Common
{
    [MeansImplicitUse]
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