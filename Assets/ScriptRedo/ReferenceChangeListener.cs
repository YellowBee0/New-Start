using System;
using System.Collections.Generic;

namespace ScriptRedo
{
    //可以使用对象池
    public sealed class ReferenceChangeListener<TKey, TValue> where TKey : notnull
    {
        private struct ActionWrap
        {
            public Action<TValue> ChangeListener;
        }

        private readonly Dictionary<TKey, ActionWrap> m_ChangeListeners = new();

        public void NotifyChange(TKey key, TValue value)
        {
            if (m_ChangeListeners.TryGetValue(key, out ActionWrap changeListener))
            {
                changeListener.ChangeListener.Invoke(value);
            }
        }

        public void RegisterChangeListener(TKey key, Action<TValue> listener)
        {
            if (listener != null)
            {
                if (!m_ChangeListeners.TryGetValue(key, out ActionWrap changeListener))
                {
                    changeListener = new ActionWrap();
                    m_ChangeListeners.Add(key, changeListener);
                }
                changeListener.ChangeListener += listener;
            }
        }

        public void UnregisterChangeListener(TKey key, Action<TValue> listener)
        {
            if (listener != null)
            {
                if (m_ChangeListeners.TryGetValue(key, out ActionWrap changeListener))
                {
                    changeListener.ChangeListener -= listener;
                    if (changeListener.ChangeListener == null)
                    {
                        m_ChangeListeners.Remove(key);
                    }
                }
            }
        }

        public void ClearAllChangeListeners()
        {
            m_ChangeListeners.Clear();
        }
    }
}