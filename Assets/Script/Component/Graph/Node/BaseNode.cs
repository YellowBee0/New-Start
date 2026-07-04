using YBFramework.Common;

namespace YBFramework.Component
{
    public abstract class BaseNode : IValueIterator<BasePort>
    {
        private ushort m_NodeID;

        public ushort GetNodeID()
        {
            return m_NodeID;
        }
        
        public BasePort GetPort(ushort portID)
        {
            foreach (BasePort port in (IValueIterator<BasePort>)this)
            {
                if (port != null && port.GetPortID() == portID)
                {
                    return port;
                }
            }
            return null;
        }

        public abstract bool Iterator(int index, out BasePort current);
    }
}