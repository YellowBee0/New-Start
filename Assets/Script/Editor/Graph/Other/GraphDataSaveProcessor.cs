/*using System.Collections.Generic;
using System.Reflection;
using Script.Common;
using UnityEditor;
using YBFramework.Bridge.NewData;
using YBFramework.Bridge.Editor;

namespace YBFramework.Editor.Graph
{
    public sealed class GraphDataSaveProcessor : AssetModificationProcessor
    {
        private readonly unsafe struct GraphDataSaveProcess
        {
            private readonly delegate*<void> m_Method;

            public GraphDataSaveProcess(delegate*<void> method)
            {
                m_Method = method;
            }

            public void Invoke()
            {
                m_Method();
            }
        }

        private static readonly Dictionary<string, GraphDataSaveProcess> s_AllSaveProcesses = new();

        private static readonly List<GraphDataSaveProcess> s_SaveProcesses = new();

        private static readonly List<string> s_GraphAssetPaths = new();

        public static IReadOnlyList<string> GetGraphAssetPaths()
        {
            return s_GraphAssetPaths;
        }

        static unsafe GraphDataSaveProcessor()
        {
            MethodInfo[] methodInfos = typeof(GraphDataSaveProcessor).GetMethods(BindingFlags.NonPublic | BindingFlags.Static);
            for (int i = 0; i < methodInfos.Length; i++)
            {
                MethodInfo methodInfo = methodInfos[i];
                MethodMarkAttribute methodMark = methodInfo.GetCustomAttribute<MethodMarkAttribute>();
                if (methodMark != null)
                {
                    delegate*<void> method = (delegate*<void>)methodInfo.MethodHandle.GetFunctionPointer();
                    s_AllSaveProcesses.Add(methodMark.MarkName, new GraphDataSaveProcess(method));
                }
            }
        }

        //TODO:蓝图资源更新逻辑
        public static void InitializeGraphAssetPaths()
        {
            string[] guids = AssetDatabase.FindAssets($"t:{nameof(GraphAsset)}");
            for (int i = 0; i < guids.Length; i++)
            {
                string graphPath = AssetDatabase.GUIDToAssetPath(guids[i]);
                s_GraphAssetPaths.Add(graphPath);
            }
        }

        [MenuItem("Tools/Migrate Graph Assets")]
        private static void MigrateSerializedData()
        {
            for (int i = 0; i < s_GraphAssetPaths.Count; i++)
            {
                string graphAssetPath = s_GraphAssetPaths[i];
                GraphAsset graphAsset = AssetDatabase.LoadAssetAtPath<GraphAsset>(graphAssetPath);
                if (graphAsset != null)
                {
                    graphAsset.InitializeBackReference();
                    IReadOnlyList<BaseNodeData> nodesData = graphAsset.GetNodesData();
                    bool isDirty = false;
                    for (int j = 0; j < nodesData.Count; j++)
                    {
                        if (nodesData[i].MigrateSerializedData(graphAsset))
                        {
                            isDirty = true;
                        }
                    }
                    if (isDirty)
                    {
                        EditorUtility.SetDirty(graphAsset);
                    }
                }
            }
            AssetDatabase.SaveAssets();
        }

        [MethodMark("Save Connect Proxy Helper Node Data")]
        private static void SaveConnectProxyHelperNodeDataProcess()
        {
            ProxyHelperPortData.GetConnectDataToSave();
        }

        [MethodMark("Save Disconnect Proxy Helper Node Data")]
        private static void SaveDisconnectProxyHelperNodeDataProcess()
        {
        }

        private static void MigrateProxyNodeSerializedData(ProxyHelperNodeData proxyHelperNodeData)
        {
            GraphAsset proxyGraphAsset = proxyHelperNodeData.GetGraphAsset();
            for (int i = 0; i < s_GraphAssetPaths.Count; i++)
            {
                string graphAssetPath = s_GraphAssetPaths[i];
                GraphAsset graphAsset = AssetDatabase.LoadAssetAtPath<GraphAsset>(graphAssetPath);
                if (graphAsset != null)
                {
                    IReadOnlyList<BaseNodeData> nodesData = graphAsset.GetNodesData();
                    bool isDirty = false;
                    for (int j = 0; j < nodesData.Count; j++)
                    {
                        if (nodesData[i] is ProxyNodeData proxyNodeData && proxyNodeData.GetProxyGraphAsset() == proxyGraphAsset)
                        {
                            //在这里迁移代理数据
                        }
                    }
                    if (isDirty)
                    {
                        EditorUtility.SetDirty(graphAsset);
                    }
                }
            }
            AssetDatabase.SaveAssets();
        }

        private static string[] OnWillSaveAssets(string[] paths)
        {
            EditorApplication.delayCall += DoProcess;
            return paths;
        }

        private static void DoProcess()
        {
            IReadOnlyList<string> processNames = GraphDataSaveProcessBridge.GetProcessNames();
            for (int i = 0; i < processNames.Count; i++)
            {
                if (s_AllSaveProcesses.TryGetValue(processNames[i], out GraphDataSaveProcess process))
                {
                    s_SaveProcesses.Add(process);
                }
            }
            for (int i = 0; i < s_SaveProcesses.Count; i++)
            {
                s_SaveProcesses[i].Invoke();
            }
            s_SaveProcesses.Clear();
        }
    }
}*/

