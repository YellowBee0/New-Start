using System.Collections.Generic;
using YBFramework.Bridge;
using YBFramework.Common;

namespace YBFramework.Component
{
    //TODO:可添加到对象池管理
    public sealed class Graph
    {
        private readonly List<BaseNode> m_Nodes = new();

        public void InitializeFromGraphAsset(GraphAsset graphAsset)
        {
            IReadOnlyList<BaseNodeData> nodeData = graphAsset.GetNodeData();
            for (int i = 0; i < nodeData.Count; i++)
            {
                BaseNode node = nodeData[i].CreateRuntimeInstance();
                if (node != null)
                {
                    m_Nodes.Add(node);
                }
            }
            for (int i = 0; i < m_Nodes.Count; i++)
            {
                foreach (BasePort port in (IValueIterator<BasePort>)m_Nodes[i])
                {
                    //TODO:端口运行时连接恢复操作
                }
            }
        }

        //TODO:每个蓝图运行状态由自己决定，而不是GraphManager决定运行（顶多在Entity启用/禁用时控制运行状态），因为buff上跑蓝图有可能会根据buff的生命周期来运行
        public void Start()
        {
        }

        public void Stop()
        {
        }
        //
    }
}