using System;
using YBFramework.Component;
#if UNITY_EDITOR
using UnityEngine;
#endif

namespace YBFramework.Bridge
{
    [Serializable]
    public abstract class BaseNodeData : IRuntimeData<BaseNode>
    {
        public ushort NodeID;

        public abstract BaseNode CreateRuntimeInstance();
#if UNITY_EDITOR
        public Vector2 Position;

        public string Name;

        public ushort SourcePortID;
#endif
    }
}