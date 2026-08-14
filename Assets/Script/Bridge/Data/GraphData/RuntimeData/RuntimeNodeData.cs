using System.Collections.Generic;

namespace YBFramework.Bridge.NewData
{
    public sealed class RuntimeNodeData
    {
        private BaseNodeData m_NodeData;

        private List<RuntimePortData> m_RuntimePortsData = new();
    }
}