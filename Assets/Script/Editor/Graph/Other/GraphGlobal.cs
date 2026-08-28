using System.Collections.Generic;
using UnityEditor;
using YBFramework.Bridge.Data;

namespace YBFramework.Editor.Graph
{
    public static class GraphGlobal
    {
        private static bool s_HasInitializedPath;

        private static bool s_HasInitializedAsset;

        private static readonly List<string> s_AllGraphAssetPaths = new();

        private static readonly List<string> s_AllGraphAssetNames = new();

        private static readonly List<GraphAsset> s_AllGraphAssets = new();

        public static void AddGraphAssetPath(string graphAssetPath)
        {
            s_AllGraphAssetPaths.Add(graphAssetPath);
        }

        public static void RemoveGraphAssetPath(string graphAssetPath)
        {
            s_AllGraphAssetPaths.Remove(graphAssetPath);
        }

        private static void InitializeGraphAssetPath()
        {
            if (!s_HasInitializedPath)
            {
                string[] guids = AssetDatabase.FindAssets($"t:{nameof(GraphAsset)}");
                for (int i = 0; i < guids.Length; i++)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                    string graphAssetName = path[(path.LastIndexOf('/') + 1)..].Split('.')[0];
                    s_AllGraphAssetPaths.Add(path);
                    s_AllGraphAssetNames.Add(graphAssetName);
                }
                s_HasInitializedPath = true;
            }
        }

        public static IReadOnlyList<string> GetAllGraphAssetPaths()
        {
            InitializeGraphAssetPath();
            return s_AllGraphAssetPaths;
        }

        public static IReadOnlyList<string> GetAllGraphAssetNames()
        {
            InitializeGraphAssetPath();
            return s_AllGraphAssetNames;
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