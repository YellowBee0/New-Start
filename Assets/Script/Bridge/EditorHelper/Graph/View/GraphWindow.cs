#if UNITY_EDITOR
using UnityEditor;

namespace YBFramework.Bridge.Editor
{
    public sealed class GraphWindow : EditorWindow
    {
        private static GraphWindow s_Instance;

        private CustomGraphView m_MainGraphView;

        [MenuItem("Window/Graph")]
        private static void Open()
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

        public CustomGraphView GetMainGraphView()
        {
            return m_MainGraphView;
        }
    }
}
#endif