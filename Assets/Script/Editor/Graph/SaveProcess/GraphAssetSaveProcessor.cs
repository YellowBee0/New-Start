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

        private static readonly Dictionary<string, Process> s_Processes;

        private static GraphAssetListener s_GraphAssetListener;

        static unsafe GraphAssetSaveProcessor()
        {
            s_Processes = new Dictionary<string, Process>();
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

        private static GraphAssetListener GetGraphAssetListener()
        {
            if (s_GraphAssetListener == null)
            {
                s_GraphAssetListener = new GraphAssetListener();
                GraphAssetPostprocessor.AddGraphAssetChangeListener(s_GraphAssetListener);
            }
            return s_GraphAssetListener;
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
                foreach (KeyValuePair<string, GraphAsset> kvp in GetGraphAssetListener())
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

        private static string[] OnWillSaveAssets(string[] paths)
        {
            EditorApplication.delayCall += DoProcess;
            return paths;
        }
    }
}