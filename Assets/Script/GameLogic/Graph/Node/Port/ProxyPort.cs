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
            //获取到实际运行时的端口后，直接赋值到子端口，然后调用MergeData合并数据，子端口也得实现MergeData函数，具体实现：强转被合并的数据为子端口数据，再调用实际端口的MergeData
            ProxyPortData proxyPortData = (ProxyPortData)dataToMerge;
            m_ProxyTargetPort.MergeData(proxyPortData.GetClonedProxyPortData());
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