using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace YBFramework.Editor.Graph
{
    //TODO:感觉不应该设置为全局的，很有可能会用到不正确的对象进行Undo，即使现在没什么问题
    public static class UndoRedoBehaviourManager
    {
        private static readonly Stack<(int groupIndex, Stack<IUndoRedoBehaviour> undoBehaviours)> s_UndoBehaviourRecords = new();

        private static readonly Stack<(int groupIndex, Stack<IUndoRedoBehaviour> redoBehaviours)> s_RedoBehaviourRecords = new();

        private static readonly List<IUndoRedoBehaviour> s_UndoRedoInvokedCache = new();

        private static IUndoRedoRecorder s_UndoRedoRecorder;

        private static Object s_LastRecordObject;

        private static int s_UndoGroupIndex;

        private static bool s_IsRecording;

        static UndoRedoBehaviourManager()
        {
            Undo.undoRedoEvent += InvokeUndoRedo;
        }

        public static void ChangeUndoRedoRecorder(IUndoRedoRecorder undoRedoRecorder)
        {
            if (undoRedoRecorder == null)
            {
                throw new ArgumentNullException(nameof(undoRedoRecorder));
            }
            if (s_UndoRedoRecorder != undoRedoRecorder)
            {
                if (s_UndoRedoRecorder != null)
                {
                    ClearUndoRedoRecords();
                }
                s_UndoRedoRecorder = undoRedoRecorder;
            }
        }

        public static void BeginRecord(string undoName)
        {
            if (s_UndoRedoRecorder == null)
            {
                Debug.LogError("Recorder is null");
                return;
            }
            if (s_IsRecording)
            {
                throw new InvalidOperationException("You have already called BeginRecord, please call EndRecord before calling BeginRecord again.");
            }
            Object recordObject = s_UndoRedoRecorder.GetRecordObject();
            if (s_LastRecordObject != recordObject)
            {
                if (s_LastRecordObject != null)
                {
                    ClearUndoRedoRecords();
                }
                s_LastRecordObject = recordObject;
            }
            if (s_LastRecordObject != null)
            {
                s_IsRecording = true;
                Undo.IncrementCurrentGroup();
                s_UndoGroupIndex = Undo.GetCurrentGroup();
                Undo.SetCurrentGroupName(undoName);
                Undo.RegisterCompleteObjectUndo(s_LastRecordObject, undoName);
                s_UndoRedoRecorder.OnBeginRecord();
            }
            else
            {
                throw new InvalidOperationException("Record unity object is null");
            }
        }

        public static void EndRecord()
        {
            if (s_IsRecording)
            {
                s_IsRecording = false;
                Undo.CollapseUndoOperations(s_UndoGroupIndex);
                s_UndoRedoRecorder.OnEndRecord();
            }
            else
            {
                throw new InvalidOperationException("You haven't called BeginRecord, please call BeginRecord before calling EndRecord.");
            }
        }

        //TODO:当蓝图子图暴露端口保存时需要调用这个清空行为栈，不实现跨蓝图修改时会Undo/Redo所有修改，减少开发难度
        public static void ClearUndoRedoRecords()
        {
            foreach ((int groupIndex, Stack<IUndoRedoBehaviour> undoBehaviours) undoBehaviourRecord in s_UndoBehaviourRecords)
            {
                foreach (IUndoRedoBehaviour undoRedoBehaviour in undoBehaviourRecord.undoBehaviours)
                {
                    IUndoRedoBehaviour.Release(undoRedoBehaviour);
                }
            }
            foreach ((int groupIndex, Stack<IUndoRedoBehaviour> redoBehaviour) redoBehaviourRecord in s_RedoBehaviourRecords)
            {
                foreach (IUndoRedoBehaviour undoRedoBehaviour in redoBehaviourRecord.redoBehaviour)
                {
                    IUndoRedoBehaviour.Release(undoRedoBehaviour);
                }
            }
            Undo.ClearAll();
            s_UndoBehaviourRecords.Clear();
            s_RedoBehaviourRecords.Clear();
            s_UndoGroupIndex = -1;
            s_IsRecording = false;
        }

        /// <summary>
        /// 压入一个UndoRedo行为，执行Undo/Redo的时候遵守先进后出的规则
        /// </summary>
        /// <param name="undoRedoBehaviour">自定义的UndoRedo行为</param>
        public static void PushUndoRedoBehaviour(IUndoRedoBehaviour undoRedoBehaviour)
        {
            s_UndoBehaviourRecords.TryPeek(out (int groupIndex, Stack<IUndoRedoBehaviour> undoBehaviours) lastUndoBehaviourRecords);
            if (lastUndoBehaviourRecords.groupIndex > s_UndoGroupIndex)
            {
                Debug.LogError($"Recorded undo group index: {lastUndoBehaviourRecords.groupIndex} was bigger than new recorded group index: {s_UndoGroupIndex}");
                return;
            }
            if (lastUndoBehaviourRecords.groupIndex < s_UndoGroupIndex)
            {
                lastUndoBehaviourRecords = new ValueTuple<int, Stack<IUndoRedoBehaviour>>(s_UndoGroupIndex, new Stack<IUndoRedoBehaviour>());
                s_UndoBehaviourRecords.Push(lastUndoBehaviourRecords);
            }
            lastUndoBehaviourRecords.undoBehaviours.Push(undoRedoBehaviour);
        }

        private static void InvokeUndoRedo(in UndoRedoInfo undoRedoInfo)
        {
            Debug.LogError($"{undoRedoInfo.undoGroup}: is redo: {undoRedoInfo.isRedo}");
            Stack<(int, Stack<IUndoRedoBehaviour>)> invokeBehaviourRecords;
            Stack<(int, Stack<IUndoRedoBehaviour>)> backInvokeBehaviourRecords;
            bool isRedo = undoRedoInfo.isRedo;
            if (isRedo)
            {
                invokeBehaviourRecords = s_RedoBehaviourRecords;
                backInvokeBehaviourRecords = s_UndoBehaviourRecords;
            }
            else
            {
                invokeBehaviourRecords = s_UndoBehaviourRecords;
                backInvokeBehaviourRecords = s_RedoBehaviourRecords;
            }
            invokeBehaviourRecords.TryPeek(out (int groupIndex, Stack<IUndoRedoBehaviour> behaviours) invokeBehaviourRecord);
            if (invokeBehaviourRecord.groupIndex == undoRedoInfo.undoGroup)
            {
                s_UndoRedoRecorder.OnUndoRedo();
                s_UndoRedoInvokedCache.Clear();
                invokeBehaviourRecord = invokeBehaviourRecords.Pop();
                while (invokeBehaviourRecord.behaviours.Count > 0)
                {
                    IUndoRedoBehaviour behaviour = invokeBehaviourRecord.behaviours.Pop();
                    if (isRedo)
                    {
                        behaviour.Redo(s_UndoRedoRecorder);
                    }
                    else
                    {
                        behaviour.Undo(s_UndoRedoRecorder);
                    }
                    s_UndoRedoInvokedCache.Add(behaviour);
                }
                for (int i = 0; i < s_UndoRedoInvokedCache.Count; i++)
                {
                    invokeBehaviourRecord.behaviours.Push(s_UndoRedoInvokedCache[i]);
                }
                backInvokeBehaviourRecords.Push(invokeBehaviourRecord);
            }
        }
    }
}