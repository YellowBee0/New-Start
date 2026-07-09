using YBFramework.Common;

namespace YBFramework.Component
{
    public abstract class BaseNode : IValueIterator<BasePort>
    {
        private int m_NodeID;

        public int GetNodeID()
        {
            return m_NodeID;
        }
        
        public BasePort GetPort(int portID)
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