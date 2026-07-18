using YBFramework.Bridge;

namespace YBFramework.GameLogic.Graph
{
    public abstract class BasePort
    {
        protected int m_PortID;

        public int GetPortID()
        {
            return m_PortID;
        }

        public virtual void MergeData(BasePortData dataToMerge)
        {
        }

        public virtual BasePort GetActualToConnectPort()
        {
            return this;
        }

        public virtual void ConnectPort(PortConnectionData portConnectionData, BasePort portToConnect)
        {
        }
    }
}