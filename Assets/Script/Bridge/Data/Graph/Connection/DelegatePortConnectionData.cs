using System;

namespace YBFramework.Bridge.Data
{
    [Serializable]
    public sealed class DelegatePortConnectionData : PortConnectionData
    {
        public bool IsExplicitCast;
    }
}