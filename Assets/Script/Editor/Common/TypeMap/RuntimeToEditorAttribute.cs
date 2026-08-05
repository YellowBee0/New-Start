using System;
using JetBrains.Annotations;

namespace YBFramework.Editor
{
    [MeansImplicitUse]
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public sealed class RuntimeToEditorAttribute : Attribute
    {
        public readonly Type RuntimeType;

        public RuntimeToEditorAttribute(Type runtimeType)
        {
            RuntimeType = runtimeType;
        }
    }
}