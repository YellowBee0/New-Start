namespace YBFramework.Bridge.NewData
{
    public sealed class SubNodeSliceData : NodeSliceData
    {
        public readonly GraphAsset SubGraphAsset;

        public readonly GraphSliceData SubGraphSliceData;

        public SubNodeSliceData(GraphAsset subGraphAsset, GraphSliceData subGraphSliceData)
        {
            SubGraphAsset = subGraphAsset;
            SubGraphSliceData = subGraphSliceData;
        }
    }
}