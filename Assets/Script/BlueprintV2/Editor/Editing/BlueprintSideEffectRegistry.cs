using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace YBFramework.BlueprintV2.Editor
{
    /// <summary>
    /// 连接全局缓存、父蓝图依赖等非 GraphAsset 状态的适配器。
    /// 普通操作可以按 ChangeSet 增量更新；Undo/Redo 必须以恢复后的资产为准重新对齐。
    /// </summary>
    public interface IBlueprintSideEffectHandler
    {
        void OnGraphChanged(BlueprintAsset asset, BlueprintChangeSet changes);

        void OnGraphRestored(BlueprintAsset asset, in UndoRedoInfo info);
    }

    /// <summary>
    /// 外部副作用的唯一广播点，避免 View、PortData 各自直接修改全局状态。
    /// </summary>
    public static class BlueprintSideEffectRegistry
    {
        private static readonly HashSet<IBlueprintSideEffectHandler> s_Handlers = new();
        private static readonly List<IBlueprintSideEffectHandler> s_Snapshot = new();

        public static void Register(IBlueprintSideEffectHandler handler)
        {
            if (handler != null)
            {
                s_Handlers.Add(handler);
            }
        }

        public static void Unregister(IBlueprintSideEffectHandler handler)
        {
            if (handler != null)
            {
                s_Handlers.Remove(handler);
            }
        }

        internal static void NotifyChanged(BlueprintAsset asset, BlueprintChangeSet changes)
        {
            SnapshotHandlers();
            for (int i = 0; i < s_Snapshot.Count; i++)
            {
                try
                {
                    s_Snapshot[i].OnGraphChanged(asset, changes);
                }
                catch (System.Exception exception)
                {
                    Debug.LogException(exception);
                }
            }
        }

        internal static void NotifyRestored(BlueprintAsset asset, in UndoRedoInfo info)
        {
            SnapshotHandlers();
            for (int i = 0; i < s_Snapshot.Count; i++)
            {
                try
                {
                    s_Snapshot[i].OnGraphRestored(asset, in info);
                }
                catch (System.Exception exception)
                {
                    Debug.LogException(exception);
                }
            }
        }

        private static void SnapshotHandlers()
        {
            // 回调中允许注册或注销 handler，所以遍历前复制，避免修改集合导致枚举异常。
            s_Snapshot.Clear();
            s_Snapshot.AddRange(s_Handlers);
        }
    }
}
