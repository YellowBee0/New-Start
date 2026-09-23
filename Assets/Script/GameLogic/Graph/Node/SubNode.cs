using System;

namespace YBFramework.GameLogic.Graph
{
    public sealed class SubNode : BaseNode
    {
        private Graph m_SubGraph;

        public void SetSubGraph(Graph graph)
        {
            m_SubGraph = graph;
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