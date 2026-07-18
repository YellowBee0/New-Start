using YBFramework.Bridge.Data;

namespace YBFramework.GameLogic.Graph
{
    public sealed class ProxyPort : BasePort
    {
        private BasePort m_ProxyTargetPort;

        public void SetProxyTargetPort(BasePort proxyTargetPort)
        {
            m_ProxyTargetPort = proxyTargetPort;
        }

        public override void MergeData(BasePortData dataToMerge)
        {
            m_ProxyTargetPort.MergeData(dataToMerge);
        }

        public override BasePort GetActualToConnectPort()
        {
            return m_ProxyTargetPort.GetActualToConnectPort();
        }

        public override void ConnectPort(PortConnectionData portConnectionData, BasePort portToConnect)
        {
            m_ProxyTargetPort.ConnectPort(portConnectionData, portToConnect);
        }
    }
}