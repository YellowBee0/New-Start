using YBFramework.Bridge.Data;

namespace YBFramework.GameLogic.Graph
{
    public abstract class BasePort
    {
        //这个字段每个端口都得保留，子端口也是一样，直接就是自身实际的端口id
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