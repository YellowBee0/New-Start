using System.Collections.Generic;
using YBFramework.GameLogic.Graph;

namespace YBFramework.Bridge.Data
{
    public static class GraphBuilder
    {
        private static readonly Dictionary<GraphAsset, HashSet<Graph>> s_BuildGraphs = new();

        public static BuildGraphData BuildGraph(GraphAsset graphAsset)
        {
            Dictionary<BaseNodeData, NodeCheckResult> graphCheckResult = GraphCheckResultManager.GetGraphCheckResult(graphAsset);
            Graph graph = new();

            BuildGraphData buildGraphData = new(graphAsset, graph, graphCheckResult.Count);

            foreach (KeyValuePair<BaseNodeData, NodeCheckResult> kvp in graphCheckResult)
            {
                BuildNodeData buildNodeData = new(kvp.Key, kvp.Value.PortCheckResults.Count);
                kvp.Key.CreateRuntimeNode(kvp.Value, buildNodeData);
                buildGraphData.AddBuildNodeData(buildNodeData);
            }


            int buildNodesDataCount = buildGraphData.GetCount();
            IReadOnlyList<BuildNodeData> buildNodesData = buildGraphData.GetBuildNodesData();
            for (int i = 0; i < buildNodesDataCount; i++)
            {
                BuildNodeData fromBuildNodeData = buildNodesData[i];
                int buildPortsDataCount = fromBuildNodeData.GetCount();
                IReadOnlyList<(BasePortData, BasePort)> buildPortsData = fromBuildNodeData.GetBuildPortsData();
                for (int j = 0; j < buildPortsDataCount; j++)
                {
                    (BasePortData fromPortData, BasePort fromPort) = buildPortsData[j];
                    int portConnectionsData = fromPortData.GetPortConnectionsDataCount();
                    for (int k = 0; k < portConnectionsData; k++)
                    {
                        PortConnectionData portConnectionData = fromPortData.PortConnectionDataOfIndex(k);
                        BuildNodeData toBuildNodeData = buildGraphData.FindBuildNodeData(portConnectionData.NodeID);
                        BasePort toBuildPort = toBuildNodeData.FindBuildPort(portConnectionData.PortID);
                        fromPort.ConnectPort(portConnectionData, toBuildPort);
                    }
                }
            }
            buildGraphData.Dispose();

            if (!s_BuildGraphs.TryGetValue(graphAsset, out HashSet<Graph> buildGraphs))
            {
                buildGraphs = new HashSet<Graph>();
                s_BuildGraphs.Add(graphAsset, buildGraphs);
            }
            buildGraphs.Add(graph);
            return buildGraphData;
        }
    }
}