using System.Collections.Generic;
using System.Reflection;
using Script.Common;
using UnityEditor;
using UnityEngine;
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

        private static readonly Dictionary<string, Process> s_Processes;

        private static readonly Dictionary<string, GraphAsset> s_GraphAssets;

        static unsafe GraphAssetSaveProcessor()
        {
            s_Processes = new Dictionary<string, Process>();
            s_GraphAssets = new Dictionary<string, GraphAsset>();
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
            HashSet<string>.Enumerator allGraphAssetPaths = GraphAssetPostprocessor.GetAllGraphAssetPaths();
            while (allGraphAssetPaths.MoveNext())
            {
                LoadGraphAsset(allGraphAssetPaths.Current);
            }
            allGraphAssetPaths.Dispose();
            GraphAssetPostprocessor.OnAddGraphAsset += OnAddGraphAsset;
            GraphAssetPostprocessor.OnRemoveGraphAsset += OnRemoveGraphAsset;
        }

        private static void LoadGraphAsset(string graphAssetPath)
        {
            GraphAsset graphAsset = AssetDatabase.LoadAssetAtPath<GraphAsset>(graphAssetPath);
            if (graphAsset != null)
            {
                s_GraphAssets.Add(graphAssetPath, graphAsset);
            }
            else
            {
                Debug.LogError($"Asset at path: {graphAssetPath} is not a {nameof(GraphAsset)} type");
            }
        }

        private static void OnAddGraphAsset(string graphAssetPath)
        {
            LoadGraphAsset(graphAssetPath);
        }

        private static void OnRemoveGraphAsset(string graphAssetPath)
        {
            s_GraphAssets.Remove(graphAssetPath);
        }

        private static string[] OnWillSaveAssets(string[] paths)
        {
            EditorApplication.delayCall += DoProcess;
            return paths;
        }

        private static void DoProcess()
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
            GraphAssetSaveProcessRegister.ClearProcessNames();
        }

        [MethodMark("Expose port data save process")]
        private static void ExposePortDataSaveProcess()
        {
            IReadOnlyList<ExposePortDataConnectionChangeData.SubGraphConnectionChangeData> subGraphConnectionsChangeData = ExposePortDataConnectionChangeData.GetSubGraphConnectionsChangeData();
            for (int i = 0; i < subGraphConnectionsChangeData.Count; i++)
            {
                ExposePortDataConnectionChangeData.SubGraphConnectionChangeData subGraphConnectionChangeData = subGraphConnectionsChangeData[i];
                foreach (KeyValuePair<string, GraphAsset> kvp in s_GraphAssets)
                {
                    GraphAsset graphAsset = kvp.Value;
                    if (graphAsset != null)
                    {
                        IReadOnlyList<BaseNodeData> nodesData = graphAsset.GetNodesData();
                        for (int j = 0; j < nodesData.Count; j++)
                        {
                            if (nodesData[j] is SubNodeData subNodeData && subNodeData.GetSubGraphAsset() == subGraphConnectionChangeData.GraphAsset)
                            {
                                IReadOnlyList<ExposePortDataConnectionChangeData.SubGraphConnectionChangeData.ConnectionChangeData> connectionsChangeData =
                                    subGraphConnectionChangeData.GetConnectionsChangeData();
                                for (int k = 0; k < connectionsChangeData.Count; k++)
                                {
                                    ExposePortDataConnectionChangeData.SubGraphConnectionChangeData.ConnectionChangeData connectionChangeData = connectionsChangeData[k];
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
}