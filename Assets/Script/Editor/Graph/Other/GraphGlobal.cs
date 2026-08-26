using System.Collections.Generic;
using UnityEditor;
using YBFramework.Bridge.NewData;

namespace YBFramework.Editor.Graph
{
    public static class GraphGlobal
    {
        private static bool s_HasInitializedPath;

        private static bool s_HasInitializedAsset;

        private static readonly List<string> s_AllGraphAssetPaths = new();

        private static readonly List<GraphAsset> s_AllGraphAssets = new();

        public static void AddGraphAssetPath(string graphAssetPath)
        {
            s_AllGraphAssetPaths.Add(graphAssetPath);
        }

        public static void RemoveGraphAssetPath(string graphAssetPath)
        {
            s_AllGraphAssetPaths.Remove(graphAssetPath);
        }

        public static IReadOnlyList<string> GetAllGraphAssetPaths()
        {
            if (!s_HasInitializedPath)
            {
                string[] guids = AssetDatabase.FindAssets($"t:{nameof(GraphAsset)}");
                for (int i = 0; i < guids.Length; i++)
                {
                    s_AllGraphAssetPaths.Add(AssetDatabase.GUIDToAssetPath(guids[i]));
                }
                s_HasInitializedPath = true;
            }
            return s_AllGraphAssetPaths;
        }

        public static void AddGraphAsset(GraphAsset graphAsset)
        {
            s_AllGraphAssets.Add(graphAsset);
        }

        public static void RemoveGraphAsset(GraphAsset graphAsset)
        {
            s_AllGraphAssets.Remove(graphAsset);
        }

        public static IReadOnlyList<GraphAsset> GetAllGraphAssets()
        {
            if (!s_HasInitializedAsset)
            {
                IReadOnlyList<string> allGraphAssetPaths = GetAllGraphAssetPaths();
                for (int i = 0; i < allGraphAssetPaths.Count; i++)
                {
                    s_AllGraphAssets.Add(AssetDatabase.LoadAssetAtPath<GraphAsset>(allGraphAssetPaths[i]));
                }
                s_HasInitializedAsset = true;
            }
            return s_AllGraphAssets;
        }
    }
}