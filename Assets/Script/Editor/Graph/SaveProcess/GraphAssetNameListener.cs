using System;
using System.Collections.Generic;
using System.IO;

namespace YBFramework.Editor.Graph
{
    public sealed class GraphAssetNameListener : IGraphAssetChangeListener
    {
        private readonly List<string> m_GraphAssetPaths = new();

        private readonly List<string> m_GraphAssetNames = new();

        private Action<string> m_OnAddGraphAsset;

        private Action<string> m_OnRemoveGraphAsset;

        public GraphAssetNameListener()
        {
            HashSet<string>.Enumerator allGraphAssetPaths = GraphAssetPostprocessor.GetAllGraphAssetPaths();
            while (allGraphAssetPaths.MoveNext())
            {
                LoadGraphAssetName(allGraphAssetPaths.Current);
            }
            allGraphAssetPaths.Dispose();
        }
        
        public IReadOnlyList<string> GetGraphAssetPaths()
        {
            return m_GraphAssetPaths;
        }

        public IReadOnlyList<string> GetGraphAssetNames()
        {
            return m_GraphAssetNames;
        }
        
        public void RegisterOnAddGraphAsset(Action<string> callback)
        {
            if (callback != null)
            {
                m_OnAddGraphAsset += callback;
            }
        }
        
        public void UnregisterOnAddGraphAsset(Action<string> callback)
        {
            m_OnAddGraphAsset -= callback;
        }
        
        public void RegisterOnRemoveGraphAsset(Action<string> callback)
        {
            if (callback != null)
            {
                m_OnRemoveGraphAsset += callback;
            }
        }
        
        public void UnregisterOnRemoveGraphAsset(Action<string> callback)
        {
            m_OnRemoveGraphAsset -= callback;
        }
        
        public string FindGraphAssetNameByPath(string graphAssetPath)
        {
            int index = m_GraphAssetPaths.IndexOf(graphAssetPath);
            if (index >= 0 && index < m_GraphAssetNames.Count)
            {
                return m_GraphAssetNames[index];
            }
            return null;
        }

        public string FindGraphAssetPathByName(string graphAssetName)
        {
            int index = m_GraphAssetNames.IndexOf(graphAssetName);
            if (index >= 0 && index < m_GraphAssetPaths.Count)
            {
                return m_GraphAssetPaths[index];
            }
            return null;
        }

        public void OnAddGraphAsset(string graphAssetPath)
        {
            LoadGraphAssetName(graphAssetPath);
            m_OnAddGraphAsset?.Invoke(graphAssetPath);
        }

        public void OnRemoveGraphAsset(string graphAssetPath)
        {
            for (int i = 0; i < m_GraphAssetPaths.Count; i++)
            {
                if (m_GraphAssetPaths[i] == graphAssetPath)
                {
                    m_GraphAssetPaths.RemoveAt(i);
                    m_GraphAssetNames.RemoveAt(i);
                }
            }
            m_OnRemoveGraphAsset?.Invoke(graphAssetPath);
        }

        private void LoadGraphAssetName(string graphAssetPath)
        {
            m_GraphAssetPaths.Add(graphAssetPath);
            m_GraphAssetNames.Add(Path.GetFileNameWithoutExtension(graphAssetPath));
        }
    }
}