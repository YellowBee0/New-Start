using System;
using YBFramework.Component;

namespace YBFramework.Bridge
{
    public interface IComponentData : IRuntimeData<IComponent>
    {
        public Type GetRuntimeInstanceType();
    }
}