using System;
using System.Collections.Generic;
using UnityEditor;

namespace YBFramework.Editor.NewGraph
{
    public sealed class GraphWindow : EditorWindow
    {
        private readonly Dictionary<string, GraphAssetDrawer> m_GraphAssetDrawers = new();

        [NonSerialized] private string m_CurrentGraphAssetPath;

        private GraphAssetDrawer m_CurrentGraphAssetDrawer;

        private void OnDestroy()
        {
            foreach (KeyValuePair<string, GraphAssetDrawer> kvp in m_GraphAssetDrawers)
            {
                kvp.Value.ClearNodeDrawers();
                GraphAssetDrawer.Release(kvp.Value);
            }
            m_GraphAssetDrawers.Clear();
            if (s_Instance == this)
            {
                s_Instance = null;
            }
            m_CurrentGraphAssetPath = null;
            m_CurrentGraphAssetDrawer = null;
        }


        #region Single instance

        private static GraphWindow s_Instance;

        private static void OpenWindow()
        {
            if (s_Instance == null)
            {
                s_Instance = GetWindow<GraphWindow>();
            }
            else
            {
                s_Instance.Focus();
            }
        }

        public static GraphWindow GetInstance()
        {
            return s_Instance;
        }

        #endregion
    }
}