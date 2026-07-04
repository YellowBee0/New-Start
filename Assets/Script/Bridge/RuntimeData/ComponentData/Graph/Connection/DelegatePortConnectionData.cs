using System;

namespace YBFramework.Bridge
{
    [Serializable]
    public sealed class DelegatePortConnectionData : PortConnectionData
    {
        public bool IsExplicitCast;
    }
}