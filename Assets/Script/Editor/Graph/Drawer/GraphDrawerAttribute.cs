using System;

namespace YBFramework.Editor.Graph
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public sealed class GraphDrawerAttribute : Attribute
    {
        public readonly Type DrawType;

        public GraphDrawerAttribute(Type drawType)
        {
            DrawType = drawType;
        }
    }
}