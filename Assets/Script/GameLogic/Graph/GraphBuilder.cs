using System;
using System.Collections.Generic;
using YBFramework.Bridge.Data;

namespace YBFramework.GameLogic.Graph
{
    public static class GraphBuilder
    {
        private static readonly Dictionary<(GraphAsset, Dictionary<BaseNodeData, NodeCheckResult>), HashSet<Graph>> s_BuildGraphs = new();

        public static Graph BuildGraph(GraphAsset graphAsset)
        {
            Dictionary<BaseNodeData, NodeCheckResult> graphCheckResult = GraphCheckResultManager.GetGraphCheckResult(graphAsset);
            Graph graph = new();
            List<BaseNode> nodes = new List<BaseNode>();
            foreach (KeyValuePair<BaseNodeData, NodeCheckResult> kvp in graphCheckResult)
            {
                //TODO:这里需要把创建的Node绑定到NodeData，同样在Node里创建的Port也需要绑定到PortData
                BaseNode node = kvp.Key.CreateRuntimeInstance(kvp.Value);
                nodes.Add(node);
            }
            graph.Initialize(nodes);
            ValueTuple<GraphAsset, Dictionary<BaseNodeData, NodeCheckResult>> graphCheckResultTuple = new(graphAsset, graphCheckResult);
            if (!s_BuildGraphs.TryGetValue(graphCheckResultTuple, out HashSet<Graph> buildGraphs))
            {
                buildGraphs = new HashSet<Graph>();
                s_BuildGraphs.Add(graphCheckResultTuple, buildGraphs);
            }
            return graph;
        }
        //需要GraphAsset和GraphAsset筛选有用数据后的集合，用于创建有用的运行时Graph数据
        //创建的运行时Graph数据，创建完Graph所有运行时数据后，需要连接对应的端口（目前就是委托和函数的连接）
    }
}