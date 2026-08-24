namespace YBFramework.Bridge.NewData
{
    public readonly struct DFSNodeData
    {
        public readonly BaseNodeData NodeData;

        public readonly NodeSliceData NodeSliceData;

        public DFSNodeData(BaseNodeData nodeData, NodeSliceData nodeSliceData)
        {
            NodeData = nodeData;
            NodeSliceData = nodeSliceData;
        }
    }
}