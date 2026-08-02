using System;

namespace YBFramework.Editor
{
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