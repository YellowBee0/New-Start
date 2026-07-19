#if UNITY_EDITOR
using System;
using YBFramework.Bridge.Data;

namespace YBFramework.Bridge.Editor
{
    [AttributeUsage(AttributeTargets.Class)]
    public abstract class NodeCreateLimitAttribute : Attribute
    {
        public abstract bool CanCreate(GraphAsset graphAsset, Type nodeType);
    }
}
#endif