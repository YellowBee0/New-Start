using System.Collections.Generic;

namespace YBFramework.Bridge.Data
{
    public sealed class ExecutableSubNodeData : ExecutableNodeData
    {
        /// <summary>
        /// 运行时有用
        /// </summary>
        public readonly GraphAsset GraphAsset;

        public readonly Dictionary<BaseNodeData, ExecutableNodeData> ExecutableNodesData;

        public ExecutableSubNodeData(GraphAsset graphAsset, Dictionary<BaseNodeData, ExecutableNodeData> nodesData)
        {
            GraphAsset = graphAsset;
            ExecutableNodesData = nodesData;
        }
    }
}