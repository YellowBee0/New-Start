using System;
using YBFramework.GameLogic.Component;

namespace YBFramework.Bridge
{
    public interface IComponentData
    {
        Type GetRuntimeInstanceType();

        IComponent CreateRuntimeInstance();
    }
}