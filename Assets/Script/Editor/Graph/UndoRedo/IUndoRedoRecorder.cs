using UnityEngine;

namespace YBFramework.Editor.Graph
{
    public interface IUndoRedoRecorder
    {
        Object GetRecordObject();

        void OnBeginRecord();

        void OnEndRecord();

        void OnUndoRedo();
    }
}