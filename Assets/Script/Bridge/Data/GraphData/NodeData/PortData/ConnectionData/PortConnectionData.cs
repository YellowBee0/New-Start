using System;

namespace YBFramework.Bridge.Data
{
    [Serializable]
    public class PortConnectionData
    {
        public int NodeID;

        public int PortID;

        public bool IsValid()
        {
            return NodeID > 0 && PortID > 0;
        }
    }
}