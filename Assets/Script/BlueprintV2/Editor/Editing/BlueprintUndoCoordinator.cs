using System;
using System.Collections.Generic;
using UnityEditor;

namespace YBFramework.BlueprintV2.Editor
{
    /// <summary>
    /// Unity 负责恢复序列化数据，本类负责发现“哪个蓝图刚被恢复”，并重建派生状态。
    /// 它不会重新执行原来的 Connect/Disconnect 命令，因为 Unity Undo 本身不提供业务命令回放。
    /// </summary>
    [InitializeOnLoad]
    internal static class BlueprintUndoCoordinator
    {
        // JSON 只用于变化检测，不用作恢复数据；真正的恢复由 Unity Undo 完成。
        private static readonly Dictionary<BlueprintAsset, string> s_Snapshots = new();
        private static readonly List<BlueprintAsset> s_Assets = new();

        static BlueprintUndoCoordinator()
        {
            Undo.undoRedoEvent += OnUndoRedo;
        }

        public static event Action<BlueprintAsset, UndoRedoInfo> GraphRestored;

        public static void Track(BlueprintAsset asset)
        {
            // 首次打开/编辑资产时记录基线，之后才能排除与本蓝图无关的 Undo 事件。
            if (asset != null && !s_Snapshots.ContainsKey(asset))
            {
                s_Snapshots.Add(asset, EditorJsonUtility.ToJson(asset));
            }
        }

        public static void Capture(BlueprintAsset asset)
        {
            // 正常业务编辑完成后保存“新状态”，下一次 Undo 才能检测到状态回退。
            if (asset != null)
            {
                s_Snapshots[asset] = EditorJsonUtility.ToJson(asset);
            }
        }

        private static void OnUndoRedo(in UndoRedoInfo info)
        {
            s_Assets.Clear();
            s_Assets.AddRange(s_Snapshots.Keys);
            for (int i = 0; i < s_Assets.Count; i++)
            {
                BlueprintAsset asset = s_Assets[i];
                if (asset == null)
                {
                    s_Snapshots.Remove(asset);
                    continue;
                }

                string current = EditorJsonUtility.ToJson(asset);
                if (string.Equals(current, s_Snapshots[asset], StringComparison.Ordinal))
                {
                    // Unity 的 Undo 是全局事件；快照未变化说明本次操作不属于该蓝图。
                    continue;
                }

                // SerializeReference 子对象可能已被替换，先恢复所有非序列化回指和索引。
                asset.RebuildNonSerializedStateInternal();
                s_Snapshots[asset] = current;
                BlueprintDirtyGraphStore.MarkDirty(asset);
                BlueprintSideEffectRegistry.NotifyRestored(asset, in info);
                // 打开的编辑器会话收到通知后，只对稳定 ID 的差异做局部视图同步。
                GraphRestored?.Invoke(asset, info);
            }
        }
    }
}
