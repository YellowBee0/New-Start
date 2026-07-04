using System;
using YBFramework.Component;

namespace YBFramework.Bridge
{
    [Serializable]
    public sealed class GraphData : IComponentData
    {
        public GraphAsset GraphAsset;

        public Type GetRuntimeInstanceType()
        {
            return typeof(Graph);
        }

        public IComponent CreateRuntimeInstance()
        {
            Graph graph = new();
            graph.InitializeFromData(this);
            return graph;
        }
    }
}