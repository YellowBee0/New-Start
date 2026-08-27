using System.Collections.Generic;
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