using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEditor;
using YBFramework.Bridge.Data;

namespace YBFramework.Editor.Graph
{
    public sealed class GraphAssetPostprocessor : AssetPostprocessor
    {
        private static readonly HashSet<string> s_AllGraphAssetPaths = new();

        public static Action<string> OnAddGraphAsset;

        public static Action<string> OnRemoveGraphAsset;

        private static bool s_HasInitialized;

        [MustDisposeResource]
        public static HashSet<string>.Enumerator GetAllGraphAssetPaths()
        {
            if (!s_HasInitialized)
            {
                s_AllGraphAssetPaths.Clear();
                string[] guids = AssetDatabase.FindAssets($"t:{nameof(GraphAsset)}");
                for (int i = 0; i < guids.Length; i++)
                {
                    string graphAssetPath = AssetDatabase.GUIDToAssetPath(guids[i]);
                    s_AllGraphAssetPaths.Add(graphAssetPath);
                }
                s_HasInitialized = true;
            }
            return s_AllGraphAssetPaths.GetEnumerator();
        }

        private static bool AddGraphAssetPath(string assetPath)
        {
            if (s_AllGraphAssetPaths.Add(assetPath))
            {
                OnAddGraphAsset?.Invoke(assetPath);
                return true;
            }
            return false;
        }

        private static bool RemoveGraphAssetPath(string assetPath)
        {
            if (s_AllGraphAssetPaths.Remove(assetPath))
            {
                OnRemoveGraphAsset?.Invoke(assetPath);
                return true;
            }
            return false;
        }

        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            if (!s_HasInitialized)
            {
                return;
            }
            for (int i = 0; i < importedAssets.Length; i++)
            {
                string assetPath = importedAssets[i];
                bool isContainsPath = s_AllGraphAssetPaths.Contains(assetPath);
                bool isGraphAsset = AssetDatabase.GetMainAssetTypeAtPath(assetPath) == typeof(GraphAsset);
                if (isContainsPath != isGraphAsset)
                {
                    AddGraphAssetPath(assetPath);
                }
            }
            for (int i = 0; i < deletedAssets.Length; i++)
            {
                RemoveGraphAssetPath(deletedAssets[i]);
            }
            for (int i = 0; i < movedFromAssetPaths.Length; i++)
            {
                if (RemoveGraphAssetPath(movedFromAssetPaths[i]))
                {
                    AddGraphAssetPath(movedAssets[i]);
                }
            }
        }
    }
}