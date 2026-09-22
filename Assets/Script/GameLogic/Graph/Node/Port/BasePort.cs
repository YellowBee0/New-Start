using YBFramework.Bridge.Data;

namespace YBFramework.GameLogic.Graph
{
    public abstract class BasePort
    {
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