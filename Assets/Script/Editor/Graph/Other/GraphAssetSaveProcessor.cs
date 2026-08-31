using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Script.Common;
using UnityEditor;
using YBFramework.Bridge.Data;
using YBFramework.Bridge.Editor;

namespace YBFramework.Editor.Graph
{
    public sealed class GraphAssetSaveProcessor : AssetModificationProcessor
    {
        private readonly unsafe struct Process
        {
            private readonly delegate*<void> m_Method;

            public Process(delegate*<void> method)
            {
                m_Method = method;
            }

            public void Invoke()
            {
                m_Method();
            }
        }

        private static readonly Dictionary<string, Process> s_Processes = new();

        private static readonly List<string> s_AllGraphAssetPaths = new();

        private static readonly List<string> s_AllGraphAssetNames = new();

        private static readonly HashSet<string> s_AllGraphAssetPathSet = new(StringComparer.Ordinal);

        private static readonly HashSet<string> s_GraphAssetDirectorySet = new(StringComparer.Ordinal);

        private static int s_GraphAssetPathInvalidationVersion = 1;

        private static int s_LoadedGraphAssetPathInvalidationVersion;

        private static int s_GraphAssetPathVersion;

        private static bool s_IsReloadingGraphAssetPaths;

        private static bool s_ProcessScheduled;

        static unsafe GraphAssetSaveProcessor()
        {
            MethodInfo[] methodInfos = typeof(GraphAssetSaveProcessor).GetMethods(BindingFlags.NonPublic | BindingFlags.Static);
            for (int i = 0; i < methodInfos.Length; i++)
            {
                MethodInfo methodInfo = methodInfos[i];
                MethodMarkAttribute methodMark = methodInfo.GetCustomAttribute<MethodMarkAttribute>();
                if (methodMark != null)
                {
                    delegate*<void> method = (delegate*<void>)methodInfo.MethodHandle.GetFunctionPointer();
                    s_Processes.Add(methodMark.MarkName, new Process(method));
                }
            }
        }

        private static string[] OnWillSaveAssets(string[] paths)
        {
            if (!s_ProcessScheduled)
            {
                s_ProcessScheduled = true;
                EditorApplication.delayCall += DoProcess;
            }
            return paths;
        }

        private static void DoProcess()
        {
            try
            {
                HashSet<string>.Enumerator enumerator = GraphAssetSaveProcessRegister.GetProcessNames();
                while (enumerator.MoveNext())
                {
                    string processName = enumerator.Current!;
                    if (s_Processes.TryGetValue(processName, out Process process))
                    {
                        process.Invoke();
                    }
                }
            }
            finally
            {
                GraphAssetSaveProcessRegister.ClearProcessNames();
                s_ProcessScheduled = false;
            }
        }

        internal static IReadOnlyList<string> GetAllGraphAssetPaths()
        {
            EnsureGraphAssetPathsLoaded();
            return s_AllGraphAssetPaths;
        }

        internal static IReadOnlyList<string> GetAllGraphAssetNames()
        {
            EnsureGraphAssetPathsLoaded();
            return s_AllGraphAssetNames;
        }

        internal static int GetGraphAssetPathVersion()
        {
            EnsureGraphAssetPathsLoaded();
            return s_GraphAssetPathVersion;
        }

        internal static void MarkGraphAssetPathsDirty()
        {
            unchecked
            {
                s_GraphAssetPathInvalidationVersion++;
            }
        }

        internal static void OnAssetsChanged(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            if (IsGraphAssetPathCacheDirty())
            {
                return;
            }
            for (int i = 0; i < deletedAssets.Length; i++)
            {
                if (IsCachedGraphAssetPathAffected(deletedAssets[i]))
                {
                    MarkGraphAssetPathsDirty();
                    return;
                }
            }
            for (int i = 0; i < movedAssets.Length; i++)
            {
                if (IsCachedGraphAssetPathAffected(movedFromAssetPaths[i]))
                {
                    MarkGraphAssetPathsDirty();
                    return;
                }
            }
            for (int i = 0; i < importedAssets.Length; i++)
            {
                string importedAssetPath = NormalizeAssetPath(importedAssets[i]);
                if (!IsSerializedAssetPath(importedAssetPath))
                {
                    continue;
                }
                bool wasGraphAsset = s_AllGraphAssetPathSet.Contains(importedAssetPath);
                bool isGraphAsset = IsGraphAssetAtPath(importedAssetPath);
                if (wasGraphAsset != isGraphAsset)
                {
                    MarkGraphAssetPathsDirty();
                    return;
                }
            }
        }

        private static void EnsureGraphAssetPathsLoaded()
        {
            if (!IsGraphAssetPathCacheDirty() || s_IsReloadingGraphAssetPaths)
            {
                return;
            }
            int invalidationVersion = s_GraphAssetPathInvalidationVersion;
            s_IsReloadingGraphAssetPaths = true;
            try
            {
                string[] guids = AssetDatabase.FindAssets($"t:{nameof(GraphAsset)}");
                List<string> loadedPaths = new(guids.Length);
                for (int i = 0; i < guids.Length; i++)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                    if (!string.IsNullOrEmpty(path))
                    {
                        loadedPaths.Add(path);
                    }
                }
                loadedPaths.Sort(StringComparer.Ordinal);

                s_AllGraphAssetPaths.Clear();
                s_AllGraphAssetNames.Clear();
                s_AllGraphAssetPathSet.Clear();
                s_GraphAssetDirectorySet.Clear();
                for (int i = 0; i < loadedPaths.Count; i++)
                {
                    string path = loadedPaths[i];
                    s_AllGraphAssetPaths.Add(path);
                    s_AllGraphAssetNames.Add(Path.GetFileNameWithoutExtension(path));
                    IndexGraphAssetPath(path);
                }

                s_LoadedGraphAssetPathInvalidationVersion = invalidationVersion;
                unchecked
                {
                    s_GraphAssetPathVersion++;
                }
            }
            finally
            {
                s_IsReloadingGraphAssetPaths = false;
            }
        }

        private static bool IsGraphAssetPathCacheDirty()
        {
            return s_LoadedGraphAssetPathInvalidationVersion != s_GraphAssetPathInvalidationVersion;
        }

        private static bool IsGraphAssetAtPath(string assetPath)
        {
            Type assetType = AssetDatabase.GetMainAssetTypeAtPath(assetPath);
            return assetType != null && typeof(GraphAsset).IsAssignableFrom(assetType);
        }

        private static bool IsCachedGraphAssetPathAffected(string changedPath)
        {
            if (string.IsNullOrEmpty(changedPath))
            {
                return false;
            }
            string normalizedChangedPath = NormalizeAssetPath(changedPath);
            return s_AllGraphAssetPathSet.Contains(normalizedChangedPath) || s_GraphAssetDirectorySet.Contains(normalizedChangedPath);
        }

        private static void IndexGraphAssetPath(string graphAssetPath)
        {
            string normalizedGraphAssetPath = NormalizeAssetPath(graphAssetPath);
            s_AllGraphAssetPathSet.Add(normalizedGraphAssetPath);
            string directoryPath = Path.GetDirectoryName(normalizedGraphAssetPath)?.Replace('\\', '/');
            while (!string.IsNullOrEmpty(directoryPath))
            {
                s_GraphAssetDirectorySet.Add(directoryPath);
                int separatorIndex = directoryPath.LastIndexOf('/');
                if (separatorIndex < 0)
                {
                    break;
                }
                directoryPath = directoryPath[..separatorIndex];
            }
        }

        private static string NormalizeAssetPath(string assetPath)
        {
            return assetPath.Replace('\\', '/').TrimEnd('/');
        }

        private static bool IsSerializedAssetPath(string assetPath)
        {
            if (string.IsNullOrEmpty(assetPath))
            {
                return false;
            }
            if (assetPath.EndsWith(".meta", StringComparison.OrdinalIgnoreCase))
            {
                assetPath = assetPath[..^5];
            }
            return assetPath.EndsWith(".asset", StringComparison.OrdinalIgnoreCase);
        }

        [MethodMark("Expose port data save process")]
        private static void ExposePortDataSaveProcess()
        {
            IReadOnlyList<ExposePortDataConnectionChangeData.SubGraphConnectionChangeData> subGraphConnectionsChangeData = ExposePortDataConnectionChangeData.GetSubGraphConnectionsChangeData();
            for (int i = 0; i < subGraphConnectionsChangeData.Count; i++)
            {
                ExposePortDataConnectionChangeData.SubGraphConnectionChangeData subGraphConnectionChangeData = subGraphConnectionsChangeData[i];
                IReadOnlyList<GraphAsset> allGraphAssets = GraphGlobal.GetAllGraphAssets();
                for (int j = 0; j < allGraphAssets.Count; j++)
                {
                    GraphAsset graphAsset = allGraphAssets[j];
                    if (graphAsset != null)
                    {
                        IReadOnlyList<BaseNodeData> nodesData = graphAsset.GetNodesData();
                        for (int k = 0; k < nodesData.Count; k++)
                        {
                            if (nodesData[k] is SubNodeData subNodeData && subNodeData.GetSubGraphAsset() == subGraphConnectionChangeData.GraphAsset)
                            {
                                IReadOnlyList<ExposePortDataConnectionChangeData.SubGraphConnectionChangeData.ConnectionChangeData> connectionsChangeData =
                                    subGraphConnectionChangeData.GetConnectionsChangeData();
                                for (int l = 0; l < connectionsChangeData.Count; l++)
                                {
                                    ExposePortDataConnectionChangeData.SubGraphConnectionChangeData.ConnectionChangeData connectionChangeData = connectionsChangeData[l];
                                    subNodeData.OnExposePortDataConnectionChanged(connectionChangeData.ExposePortData, connectionChangeData.ToExposeNodeID, connectionChangeData.ToExposePortID,
                                        connectionChangeData.IsConnect);
                                }
                                //判断被修改数据的蓝图是否创建过视图，如果存在创建的视图就销毁，下一次查看时创建新的
                                GraphWindow instance = GraphWindow.GetInstance();
                                if (instance != null)
                                {
                                    instance.DestroyGraphView(AssetDatabase.GetAssetPath(graphAsset));
                                    graphAsset.SetDirtyToReinitialize();
                                }
                                EditorUtility.SetDirty(graphAsset);
                            }
                        }
                    }
                }
            }
            ExposePortDataConnectionChangeData.Clear();
            AssetDatabase.SaveAssets();
        }
    }

    internal sealed class GraphAssetPostprocessor : AssetPostprocessor
    {
        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            GraphAssetSaveProcessor.OnAssetsChanged(importedAssets, deletedAssets, movedAssets, movedFromAssetPaths);
        }
    }
}
