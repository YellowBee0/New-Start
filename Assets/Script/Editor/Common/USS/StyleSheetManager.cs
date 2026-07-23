using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace YBFramework.Editor
{
    public static class StyleSheetManager
    {
        private const string USS_PATH = "Assets/USS/";

        private static readonly Dictionary<string, StyleSheet> s_StyleSheets = new();

        public static StyleSheet LoadStylesheet(string name)
        {
            if (!s_StyleSheets.TryGetValue(name, out StyleSheet styleSheet))
            {
                string path = USS_PATH + name + ".uss";
                styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(path);
                if (styleSheet)
                {
                    s_StyleSheets.Add(name, styleSheet);
                }
                else
                {
                    Debug.LogError($"Can not load stylesheet {name} at path {path}");
                }
            }
            return styleSheet;
        }
    }
}