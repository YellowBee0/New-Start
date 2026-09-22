using YBFramework.Bridge.Data;

namespace YBFramework.GameLogic.Graph
{
    public sealed class TestNode : BaseNode
    {
        private ActionPort m_ActionPort;
        
        private ValueInputPort<int> m_IntValueInputPort;


        public void CreatePorts(TestNodeData testNodeData, NodeCheckResult nodeCheckResult)
        {
            if (nodeCheckResult.PortCheckResults.Contains(testNodeData.InvokePortData))
            {
                m_ActionPort = (ActionPort)testNodeData.InvokePortData.CreateRuntimeInstance();
            }
            if (nodeCheckResult.PortCheckResults.Contains(testNodeData.IntInputPort))
            {
                m_IntValueInputPort = (ValueInputPort<int>)testNodeData.IntInputPort.CreateRuntimeInstance();
            }
        }
        
        public override void OnStart()
        {
            throw new System.NotImplementedException();
        }

        public override void OnStop()
        {
            throw new System.NotImplementedException();
        }

        public override void OnReset()
        {
            throw new System.NotImplementedException();
        }
    }
}