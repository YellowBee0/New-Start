#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using YBFramework.Bridge.Data;

namespace YBFramework.Bridge.Editor
{
    public sealed class CustomGraphView : GraphView
    {
        private GraphAsset m_GraphAsset;

        public void CreateNodeView((Type, IEnumerable<NodeCreateLimitAttribute>) nodeMetaData)
        {
            if (nodeMetaData.Item1 == null)
            {
                return;
            }
            foreach (NodeCreateLimitAttribute nodeCreateLimitAttribute in nodeMetaData.Item2)
            {
                if (!nodeCreateLimitAttribute.CanCreate(m_GraphAsset, nodeMetaData.Item1))
                {
                    return;
                }
            }
            if (Activator.CreateInstance(nodeMetaData.Item1) is BaseNodeData nodeData)
            {
                
            }
        }
    }
}
#endif