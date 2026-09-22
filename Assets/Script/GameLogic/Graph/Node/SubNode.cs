using System;
using YBFramework.Bridge.Data;

namespace YBFramework.GameLogic.Graph
{
    public sealed class SubNode : BaseNode
    {
        private Graph m_SubGraph;

        public void Create(SubNodeData subNodeData, NodeCheckResult nodeCheckResult)
        {
            m_SubGraph = GraphBuilder.BuildGraph(subNodeData.GetSubGraphAsset());
            int portsDataCount = subNodeData.GetPortsDataCount();
            for (int i = 0; i < portsDataCount; i++)
            {
                BasePortData portData = subNodeData.PortDataOfIndex(i);
                if (nodeCheckResult.PortCheckResults.Contains(portData))
                {
                    BasePort subPort = portData.CreateRuntimeInstance();
                    //添加到待连接集合
                    //找到实际运行端口，复制运行时数据
                }
            }
        }
        
        public override void OnStart()
        {
            m_SubGraph.Start();
        }

        public override void OnStop()
        {
            m_SubGraph.Stop();
        }

        public override void OnReset()
        {
            throw new NotImplementedException();
        }
    }
}