using System;
using YBFramework.Common;
using YBFramework.GameLogic.Graph;
#if UNITY_EDITOR
using UnityEngine;
#endif

namespace YBFramework.Bridge
{
    [Serializable]
    public abstract class BaseNodeData : IRuntimeData<BaseNode>, IValueIterator<BasePortData>
    {
        public int NodeID;

        public abstract BaseNode CreateRuntimeInstance();

        public abstract bool Iterator(int index, out BasePortData current);
#if UNITY_EDITOR
        public Vector2 Position;

        public string Name;

        public int SourcePortID;
#endif
    }
}