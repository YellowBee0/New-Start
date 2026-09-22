using System;
using System.Buffers;
using System.Collections.Generic;
using YBFramework.Bridge.Data;

namespace YBFramework.GameLogic.Graph
{
    public static class GraphBuilder
    {
        private static readonly Dictionary<GraphAsset, HashSet<Graph>> s_BuildGraphs = new();

        private ref struct GraphAssetMapping
        {
            private readonly (BaseNodeData, NodeDataMapping)[] NodeDataMappings;

            private int m_Count;

            public GraphAssetMapping(int size)
            {
                NodeDataMappings = ArrayPool<(BaseNodeData, NodeDataMapping)>.Shared.Rent(size);
                m_Count = 0;
            }

            public int GetCount()
            {
                return m_Count;
            }

            public IReadOnlyList<(BaseNodeData, NodeDataMapping)> GetNodeDataMappings()
            {
                return NodeDataMappings;
            }

            public NodeDataMapping FindNodeDataMapping(int nodeID)
            {
                for (int i = 0; i < m_Count; i++)
                {
                    (BaseNodeData, NodeDataMapping) nodeDataMapping = NodeDataMappings[i];
                    if (nodeDataMapping.Item1.GetNodeID() == nodeID)
                    {
                        return nodeDataMapping.Item2;
                    }
                }
                return default;
            }

            public void AddNodeDataMapping(BaseNodeData nodeData, NodeDataMapping nodeDataMapping)
            {
                NodeDataMappings[m_Count++] = new ValueTuple<BaseNodeData, NodeDataMapping>(nodeData, nodeDataMapping);
            }

            public void Clear()
            {
                for (int i = 0; i < m_Count; i++)
                {
                    NodeDataMappings[i].Item2.Clear();
                }
                ArrayPool<(BaseNodeData, NodeDataMapping)>.Shared.Return(NodeDataMappings, true);
            }
        }

        private struct NodeDataMapping : IEquatable<NodeDataMapping>
        {
            private readonly (BasePortData, BasePort)[] PortDataMappings;

            private int m_Count;

            public NodeDataMapping(int size)
            {
                PortDataMappings = ArrayPool<(BasePortData, BasePort)>.Shared.Rent(size);
                m_Count = 0;
            }

            public int GetCount()
            {
                return m_Count;
            }

            public IReadOnlyList<(BasePortData, BasePort)> GetPortDataMappings()
            {
                return PortDataMappings;
            }

            public BasePort FindPort(int portID)
            {
                for (int i = 0; i < PortDataMappings.Length; i++)
                {
                    (BasePortData, BasePort) nodeDataMapping = PortDataMappings[i];
                    if (nodeDataMapping != default && nodeDataMapping.Item1.GetPortID() == portID)
                    {
                        return nodeDataMapping.Item2;
                    }
                }
                return null;
            }

            public void AddNodeDataMapping(BasePortData portData, BasePort port)
            {
                PortDataMappings[m_Count++] = new ValueTuple<BasePortData, BasePort>(portData, port);
            }

            public void Clear()
            {
                ArrayPool<(BasePortData, BasePort)>.Shared.Return(PortDataMappings, true);
            }

            public bool Equals(NodeDataMapping other)
            {
                return Equals(PortDataMappings, other.PortDataMappings);
            }

            public override bool Equals(object obj)
            {
                return obj is NodeDataMapping other && Equals(other);
            }

            public override int GetHashCode()
            {
                return (PortDataMappings != null ? PortDataMappings.GetHashCode() : 0);
            }

            public static bool operator ==(NodeDataMapping left, NodeDataMapping right)
            {
                return left.PortDataMappings == right.PortDataMappings;
            }

            public static bool operator !=(NodeDataMapping left, NodeDataMapping right)
            {
                return !(left == right);
            }
        }

        public static Graph BuildGraph(GraphAsset graphAsset)
        {
            Dictionary<BaseNodeData, NodeCheckResult> graphCheckResult = GraphCheckResultManager.GetGraphCheckResult(graphAsset);
            Graph graph = new();

            GraphAssetMapping graphAssetMapping = new(graphCheckResult.Count);
            
            List<BaseNode> nodes = new();
            foreach (KeyValuePair<BaseNodeData, NodeCheckResult> kvp in graphCheckResult)
            {
                NodeDataMapping nodeDataMapping = new(kvp.Value.PortCheckResults.Count);
                //TODO:这里需要把创建的Node绑定到NodeData，同样在Node里创建的Port也需要绑定到PortData
                BaseNode node = kvp.Key.CreateRuntimeInstance(kvp.Value);
                nodes.Add(node);
                graphAssetMapping.AddNodeDataMapping(kvp.Key, nodeDataMapping);
            }
            graph.Initialize(nodes);

            int nodeDataMappingsCount = graphAssetMapping.GetCount();
            IReadOnlyList<(BaseNodeData, NodeDataMapping)> nodeDataMappings = graphAssetMapping.GetNodeDataMappings();
            for (int i = 0; i < nodeDataMappingsCount; i++)
            {
                NodeDataMapping fromNodeDataMapping = nodeDataMappings[i].Item2;
                int portDataMappingsCount = fromNodeDataMapping.GetCount();
                IReadOnlyList<(BasePortData, BasePort)> portDataMappings = fromNodeDataMapping.GetPortDataMappings();
                for (int j = 0; j < portDataMappingsCount; j++)
                {
                    (BasePortData fromPortData, BasePort fromPort) = portDataMappings[j];
                    int portConnectionsData = fromPortData.GetPortConnectionsDataCount();
                    for (int k = 0; k < portConnectionsData; k++)
                    {
                        PortConnectionData portConnectionData = fromPortData.PortConnectionDataOfIndex(k);
                        NodeDataMapping toNodeDataMapping = graphAssetMapping.FindNodeDataMapping(portConnectionData.NodeID);
                        BasePort toPort = toNodeDataMapping.FindPort(portConnectionData.PortID);
                        fromPort.ConnectPort(portConnectionData, toPort);
                    }
                }
            }
            graphAssetMapping.Clear();

            if (!s_BuildGraphs.TryGetValue(graphAsset, out HashSet<Graph> buildGraphs))
            {
                buildGraphs = new HashSet<Graph>();
                s_BuildGraphs.Add(graphAsset, buildGraphs);
            }
            buildGraphs.Add(graph);
            return graph;
        }
        //需要GraphAsset和GraphAsset筛选有用数据后的集合，用于创建有用的运行时Graph数据
        //创建的运行时Graph数据，创建完Graph所有运行时数据后，需要连接对应的端口（目前就是委托和函数的连接）
    }
}