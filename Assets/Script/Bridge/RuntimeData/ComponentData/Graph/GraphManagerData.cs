using System;
using System.Collections.Generic;
using UnityEngine;
using YBFramework.Component;

namespace YBFramework.Bridge
{
    [Serializable]
    public sealed class GraphManagerData : IComponentData
    {
        [SerializeField] private List<GraphAsset> m_GraphAssets;

        public IReadOnlyList<GraphAsset> GetGraphAssets()
        {
            return m_GraphAssets;
        }
        
        public IComponent CreateRuntimeInstance()
        {
            GraphManager graphManager = new();
            graphManager.InitializeFromData(this);
            return graphManager;
        }

        public Type GetRuntimeInstanceType()
        {
            return typeof(GraphManager);
        }
    }
}