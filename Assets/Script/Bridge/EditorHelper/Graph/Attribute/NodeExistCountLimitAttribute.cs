#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEngine;
using YBFramework.Bridge.Data;

namespace YBFramework.Bridge.Editor
{
    public sealed class NodeExistCountLimitAttribute : NodeCreateLimitAttribute
    {
        private readonly int m_LimitCount;

        public NodeExistCountLimitAttribute(int limitCount)
        {
            m_LimitCount = limitCount;
        }

        public override bool CanCreate(GraphAsset graphAsset, Type nodeType)
        {
            int existCount = 0;
            IReadOnlyList<BaseNodeData> nodeData = graphAsset.GetNodesData();
            for (int i = 0; i < nodeData.Count; i++)
            {
                if (nodeData[i].GetType().IsAssignableFrom(nodeType))
                {
                    if (++existCount >= m_LimitCount)
                    {
                        Debug.LogError($"Graph {graphAsset.name} can not create {nodeType} more than {m_LimitCount}");
                        return false;
                    }
                }
            }
            return true;
        }
    }
}
#endif