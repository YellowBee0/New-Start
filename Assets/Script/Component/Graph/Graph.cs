using System.Collections.Generic;
using YBFramework.Bridge;
using YBFramework.Common;

namespace YBFramework.Component
{
    //TODO:可添加到对象池管理
    public sealed class Graph
    {
        private readonly List<BaseNode> m_Nodes = new();
        
        //TODO:新增蓝图运行选项应该是一个Flags的枚举。Entity启用/禁用时、其他地方控制（是否需要细分）（比如buff生命周期控制）。
        // 这个枚举支持动态设置。比如在编辑器中并没有设置运行选项

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