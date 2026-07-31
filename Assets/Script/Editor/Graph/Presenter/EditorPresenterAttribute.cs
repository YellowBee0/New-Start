using System;

namespace YBFramework.Editor
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public sealed class EditorPresenterAttribute : Attribute
    {
        public readonly Type RuntimeType;

        public EditorPresenterAttribute(Type runtimeType)
        {
            RuntimeType = runtimeType;
        }
    }
}