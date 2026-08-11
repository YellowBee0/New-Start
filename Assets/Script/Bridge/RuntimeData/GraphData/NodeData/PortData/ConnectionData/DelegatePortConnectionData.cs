using System;

namespace YBFramework.Bridge.NewData
{
    [Serializable]
    public sealed class DelegatePortConnectionData : PortConnectionData
    {
        public bool IsExplicitCast;
    }
}