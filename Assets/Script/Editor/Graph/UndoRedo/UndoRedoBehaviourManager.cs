using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace YBFramework.Editor.Graph
{
    public static class UndoRedoBehaviourManager
    {
        private static readonly Dictionary<int, Stack<IUndoRedoBehaviour>> s_UndoBehaviours = new();

        private static readonly Dictionary<int, Stack<IUndoRedoBehaviour>> s_RedoBehaviours = new();

        private static Stack<IUndoRedoBehaviour> s_UndoRedoInvokeStack = new();

        static UndoRedoBehaviourManager()
        {
            Undo.undoRedoEvent += InvokeUndoRedo;
        }

        /// <summary>
        /// 压入一个UndoRedo行为，执行Undo/Redo的时候遵守先进后出的规则
        /// </summary>
        /// <param name="groupIndex">Undo记录的GroupIndex</param>
        /// <param name="undoRedoBehaviour">自定义的UndoRedo行为</param>
        public static void PushUndoRedoBehaviour(int groupIndex, IUndoRedoBehaviour undoRedoBehaviour)
        {
            if (!s_UndoBehaviours.TryGetValue(groupIndex, out Stack<IUndoRedoBehaviour> undoRedoBehaviours))
            {
                undoRedoBehaviours = new Stack<IUndoRedoBehaviour>();
                s_UndoBehaviours.Add(groupIndex, undoRedoBehaviours);
            }
            undoRedoBehaviours.Push(undoRedoBehaviour);
        }

        public static void Clear()
        {
            foreach (KeyValuePair<int, Stack<IUndoRedoBehaviour>> kvp in s_UndoBehaviours)
            {
                foreach (IUndoRedoBehaviour undoRedoBehaviour in kvp.Value)
                {
                    IUndoRedoBehaviour.Release(undoRedoBehaviour);
                }
            }
            foreach (KeyValuePair<int, Stack<IUndoRedoBehaviour>> kvp in s_RedoBehaviours)
            {
                foreach (IUndoRedoBehaviour undoRedoBehaviour in kvp.Value)
                {
                    IUndoRedoBehaviour.Release(undoRedoBehaviour);
                }
            }
            s_UndoBehaviours.Clear();
            s_RedoBehaviours.Clear();
        }

        private static void InvokeUndoRedo(in UndoRedoInfo undoRedoInfo)
        {
            Debug.LogError($"{undoRedoInfo.undoGroup}: is redo: {undoRedoInfo.isRedo}");
            if (undoRedoInfo.isRedo)
            {
                //可以统一函数
                if (s_RedoBehaviours.TryGetValue(undoRedoInfo.undoGroup, out Stack<IUndoRedoBehaviour> redoBehaviours))
                {
                    Stack<IUndoRedoBehaviour> undoBehaviours = new();
                    while (redoBehaviours.Count > 0)
                    {
                        IUndoRedoBehaviour redoBehaviour = redoBehaviours.Pop();
                        redoBehaviour.Redo();
                        undoBehaviours.Push(redoBehaviour);
                    }
                    s_RedoBehaviours.Remove(undoRedoInfo.undoGroup);
                    s_UndoBehaviours.Add(undoRedoInfo.undoGroup, undoBehaviours);
                }
            }
            else
            {
                //可以统一函数
                if (s_UndoBehaviours.TryGetValue(undoRedoInfo.undoGroup, out Stack<IUndoRedoBehaviour> undoBehaviours))
                {
                    //Stack集合可以用中间集合保存
                    Stack<IUndoRedoBehaviour> redoBehaviours = new();
                    while (undoBehaviours.Count > 0)
                    {
                        IUndoRedoBehaviour undoBehaviour = undoBehaviours.Pop();
                        undoBehaviour.Undo();
                        redoBehaviours.Push(undoBehaviour);
                    }
                    s_UndoBehaviours.Remove(undoRedoInfo.undoGroup);
                    s_RedoBehaviours.Add(undoRedoInfo.undoGroup, redoBehaviours);
                }
            }
        }
    }
}