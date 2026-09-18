namespace YBFramework.Bridge.Data
{
    public readonly struct CheckNodeExecutionContext
    {
        public readonly BaseNodeData NodeData;

        public readonly ExecutableNodeData ExecutableNodeData;

        public CheckNodeExecutionContext(BaseNodeData nodeData, ExecutableNodeData executableNodeData)
        {
            NodeData = nodeData;
            ExecutableNodeData = executableNodeData;
        }
    }
}