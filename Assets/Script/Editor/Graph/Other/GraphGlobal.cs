using System.Collections.Generic;
using UnityEditor;
using YBFramework.Bridge.Data;

namespace YBFramework.Editor.Graph
{
    public static class GraphGlobal
    {
        private static int s_LoadedGraphAssetPathVersion = -1;

        private static readonly List<GraphAsset> s_AllGraphAssets = new();

        public static void AddGraphAssetPath(string graphAssetPath)
        {
            GraphAssetSaveProcessor.MarkGraphAssetPathsDirty();
        }

        public static void RemoveGraphAssetPath(string graphAssetPath)
        {
            GraphAssetSaveProcessor.MarkGraphAssetPathsDirty();
        }

        public static IReadOnlyList<string> GetAllGraphAssetPaths()
        {
            return GraphAssetSaveProcessor.GetAllGraphAssetPaths();
        }

        public static IReadOnlyList<string> GetAllGraphAssetNames()
        {
            return GraphAssetSaveProcessor.GetAllGraphAssetNames();
        }



        public static void AddGraphAsset(GraphAsset graphAsset)
        {
            if (graphAsset == null)
            {
                return;
            }
            GetAllGraphAssets();
            if (!s_AllGraphAssets.Contains(graphAsset))
            {
                s_AllGraphAssets.Add(graphAsset);
            }
        }

        public static void RemoveGraphAsset(GraphAsset graphAsset)
        {
            GetAllGraphAssets();
            s_AllGraphAssets.Remove(graphAsset);
        }

        public static IReadOnlyList<GraphAsset> GetAllGraphAssets()
        {
            IReadOnlyList<string> allGraphAssetPaths = GraphAssetSaveProcessor.GetAllGraphAssetPaths();
            int graphAssetPathVersion = GraphAssetSaveProcessor.GetGraphAssetPathVersion();
            if (s_LoadedGraphAssetPathVersion != graphAssetPathVersion)
            {
                List<GraphAsset> loadedGraphAssets = new(allGraphAssetPaths.Count);
                for (int i = 0; i < allGraphAssetPaths.Count; i++)
                {
                    GraphAsset graphAsset = AssetDatabase.LoadAssetAtPath<GraphAsset>(allGraphAssetPaths[i]);
                    if (graphAsset != null)
                    {
                        loadedGraphAssets.Add(graphAsset);
                    }
                }
                s_AllGraphAssets.Clear();
                s_AllGraphAssets.AddRange(loadedGraphAssets);
                s_LoadedGraphAssetPathVersion = graphAssetPathVersion;
            }
            return s_AllGraphAssets;
        }
    }
}
