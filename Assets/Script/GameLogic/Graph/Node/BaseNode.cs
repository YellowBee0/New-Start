using YBFramework.Common;

namespace YBFramework.GameLogic.Graph
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

        public abstract void OnStart();

        public abstract void OnStop();

        public abstract void OnReset();
    }
}