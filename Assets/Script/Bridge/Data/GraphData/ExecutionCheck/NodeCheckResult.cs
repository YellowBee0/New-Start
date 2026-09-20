using System.Collections.Generic;

namespace YBFramework.Bridge.Data
{
    public class NodeCheckResult
    {
        public readonly HashSet<BasePortData> PortCheckResults = new();
    }
}