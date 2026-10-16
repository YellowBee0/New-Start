using System;

namespace ScriptRedo
{
    public sealed class PoolAnalyzerAdapter : IPoolAnalyzer
    {
        public Type ObjectType;

        public int TotalCount;

        public int IdleCount;

        public Type GetObjectType()
        {
            return ObjectType;
        }

        public int GetTotalObjectCount()
        {
            return TotalCount;
        }

        public int GetIdleObjectCount()
        {
            return IdleCount;
        }
    }
}