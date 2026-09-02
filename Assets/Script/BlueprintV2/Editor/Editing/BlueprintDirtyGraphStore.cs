using System.Collections.Generic;
using UnityEditor;

namespace YBFramework.BlueprintV2.Editor
{
    /// <summary>
    /// 持久化的“待外部同步”队列。
    /// 只保存 Asset GUID，因此不会持有已卸载对象，也能跨 Domain Reload/编辑器重启继续处理。
    /// </summary>
    [FilePath("Library/BlueprintV2/DirtyGraphs.asset", FilePathAttribute.Location.ProjectFolder)]
    public sealed class BlueprintDirtyGraphStore : ScriptableSingleton<BlueprintDirtyGraphStore>
    {
        [UnityEngine.SerializeField] private List<string> m_GraphGuids = new();

        public static IReadOnlyList<string> DirtyGraphGuids => instance.m_GraphGuids;

        public static void MarkDirty(BlueprintAsset asset)
        {
            if (asset == null)
            {
                return;
            }
            string path = AssetDatabase.GetAssetPath(asset);
            if (string.IsNullOrEmpty(path))
            {
                return;
            }
            string guid = AssetDatabase.AssetPathToGUID(path);
            // 正常编辑、Undo 和 Redo 都会走到这里；重复标记不会反复写入列表。
            if (!string.IsNullOrEmpty(guid) && !instance.m_GraphGuids.Contains(guid))
            {
                instance.m_GraphGuids.Add(guid);
                instance.Save(true);
            }
        }

        public static void Clear(string graphGuid)
        {
            if (instance.m_GraphGuids.Remove(graphGuid))
            {
                instance.Save(true);
            }
        }

        public static void ClearAll()
        {
            if (instance.m_GraphGuids.Count == 0)
            {
                return;
            }
            instance.m_GraphGuids.Clear();
            instance.Save(true);
        }
    }
}
