using System.Collections.Generic;
using System.Reflection;
using Script.Common;
using UnityEditor;
using YBFramework.Bridge.Editor;
using YBFramework.Bridge.Data;

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
            IReadOnlyList<string> processNames = GraphDataSaveProcessBridge.GetProcessNames();
            for (int i = 0; i < processNames.Count; i++)
            {
                if (s_Processes.TryGetValue(processNames[i], out Process process))
                {
                    process.Invoke();
                }
            }
            GraphDataSaveProcessBridge.ClearProcessNames();
        }

        [MethodMark("Sub port data save process")]
        private static void SubPortDataSaveProcess()
        {
            IReadOnlyList<SubPortDataBridgeConnectionChangeData.SubGraphConnectionChangeData> subGraphConnectionsChangeData = SubPortDataBridgeConnectionChangeData.GetSubGraphConnectionsChangeData();
            for (int i = 0; i < subGraphConnectionsChangeData.Count; i++)
            {
                SubPortDataBridgeConnectionChangeData.SubGraphConnectionChangeData subGraphConnectionChangeData = subGraphConnectionsChangeData[i];
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
                                IReadOnlyList<SubPortDataBridgeConnectionChangeData.SubGraphConnectionChangeData.ConnectionChangeData> connectionsChangeData =
                                    subGraphConnectionChangeData.GetConnectionsChangeData();
                                for (int l = 0; l < connectionsChangeData.Count; l++)
                                {
                                    SubPortDataBridgeConnectionChangeData.SubGraphConnectionChangeData.ConnectionChangeData connectionChangeData = connectionsChangeData[l];
                                    subNodeData.OnSubPortDataBridgeConnectionChanged(connectionChangeData.PortData, connectionChangeData.SubNodeID, connectionChangeData.SubPortID,
                                        connectionChangeData.IsConnect);
                                }
                                EditorUtility.SetDirty(graphAsset);
                            }
                        }
                    }
                }
            }
            SubPortDataBridgeConnectionChangeData.Clear();
            AssetDatabase.SaveAssets();
        }
    }
}