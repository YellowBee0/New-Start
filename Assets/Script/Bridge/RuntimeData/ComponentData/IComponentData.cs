using System;
using YBFramework.GameLogic.Component;

namespace YBFramework.Bridge.Data
{
    public interface IComponentData
    {
        Type GetRuntimeInstanceType();

        IComponent CreateRuntimeInstance();
    }
}