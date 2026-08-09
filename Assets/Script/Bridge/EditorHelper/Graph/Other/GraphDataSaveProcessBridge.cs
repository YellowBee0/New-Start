#if UNITY_EDITOR
using System.Collections.Generic;

namespace YBFramework.Bridge.Editor
{
    public static class GraphDataSaveProcessBridge
    {
        private static readonly List<string> m_ProcessNames = new();

        public static IReadOnlyList<string> GetProcessNames()
        {
            return m_ProcessNames;
        }

        public static void RegisterProcess(string processName)
        {
            m_ProcessNames.Add(processName);
        }

        public static void UnregisterProcess(string processName)
        {
            m_ProcessNames.Remove(processName);
        }

        public static void ClearProcessNames()
        {
            m_ProcessNames.Clear();
        }
    }
}
#endif