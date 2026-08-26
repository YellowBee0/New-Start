#if UNITY_EDITOR
using System.Collections.Generic;

namespace YBFramework.Bridge.Editor
{
    public static class GraphAssetSaveProcessRegister
    {
        private static readonly HashSet<string> m_ProcessNames = new();

        public static HashSet<string>.Enumerator GetProcessNames()
        {
            return m_ProcessNames.GetEnumerator();
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