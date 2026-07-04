using System;
using YBFramework.Component;

namespace YBFramework.Bridge
{
    [Serializable]
    public abstract class BaseNodeData : IRuntimeData<BaseNode>
    {
        public ushort NodeID;

        public abstract BaseNode CreateRuntimeInstance();
    }
}