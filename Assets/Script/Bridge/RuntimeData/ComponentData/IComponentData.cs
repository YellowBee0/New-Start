using System;
using YBFramework.GameLogic.Component;

namespace YBFramework.Bridge
{
    public interface IComponentData : IRuntimeData<IComponent>
    {
        public Type GetRuntimeInstanceType();
    }
}