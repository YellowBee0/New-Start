using System.Collections.Generic;

namespace YBFramework.Bridge.NewData
{
    public sealed class RuntimeGraphData
    {
        private GraphAsset m_GraphAsset;
        
        private readonly List<RuntimeNodeData> m_RuntimeNodesData = new();
    }
}