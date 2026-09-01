using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using YBFramework.Bridge.Data;

namespace YBFramework.Editor.Graph
{
    public sealed class GraphAssetListener : IGraphAssetChangeListener
    {
        private readonly Dictionary<string, GraphAsset> m_GraphAssets = new();

        public GraphAssetListener()
        {
            HashSet<string>.Enumerator allGraphAssetPaths = GraphAssetPostprocessor.GetAllGraphAssetPaths();
            while (allGraphAssetPaths.MoveNext())
            {
                LoadGraphAsset(allGraphAssetPaths.Current);
            }
            allGraphAssetPaths.Dispose();
        }

        public Dictionary<string, GraphAsset>.Enumerator GetEnumerator()
        {
            return m_GraphAssets.GetEnumerator();
        }

        public void OnAddGraphAsset(string graphAssetPath)
        {
            LoadGraphAsset(graphAssetPath);
        }

        public void OnRemoveGraphAsset(string graphAssetPath)
        {
            m_GraphAssets.Remove(graphAssetPath);
        }

        private void LoadGraphAsset(string graphAssetPath)
        {
            GraphAsset graphAsset = AssetDatabase.LoadAssetAtPath<GraphAsset>(graphAssetPath);
            if (graphAsset != null)
            {
                m_GraphAssets.Add(graphAssetPath, graphAsset);
            }
            else
            {
                Debug.LogError($"Asset at path: {graphAssetPath} is not a {nameof(GraphAsset)} type");
            }
        }
    }
}