using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using YBFramework.Bridge.Data;

namespace YBFramework.Editor.Graph
{
    public sealed class GraphWindow : EditorWindow
    {
        private readonly Dictionary<string, GraphAssetDrawer> m_GraphAssetDrawers = new();

        private readonly List<(string path, string name)> m_SearchItems = new();

        private readonly List<(string path, string name)> m_FilteredItems = new();

        private GraphAssetDrawer m_CurrentGraphAssetDrawer;

        private ListView m_ListView;

        private VisualElement m_GraphContainer;

        [NonSerialized] private string m_CurrentGraphAssetPath;

        [NonSerialized] private string m_SearchStr;

        [NonSerialized] private bool m_HasInitializedGraphAssetName;

        #region List view

        private VisualElement MakeItem()
        {
            Label label = new()
            {
                //设置内容超出父元素后的显示方式
                style =
                {
                    overflow = Overflow.Hidden,
                    whiteSpace = WhiteSpace.NoWrap,
                    textOverflow = TextOverflow.Ellipsis
                }
            };
            label.RegisterCallback<MouseDownEvent>(OnLabelClicked);
            return label;
        }

        private void OnLabelClicked(MouseDownEvent evt)
        {
            if (evt.target is VisualElement target)
            {
                ChangeMainGraphView((string)target.userData);
            }
        }

        private void BindItem(VisualElement item, int index)
        {
            if (item is Label label)
            {
                (string path, string name) tuple = m_FilteredItems[index];
                label.text = tuple.name;
                label.userData = tuple.path;
            }
        }

        private void OnSearchStrChanged(ChangeEvent<string> evt)
        {
            if (m_SearchStr != evt.newValue)
            {
                m_SearchStr = evt.newValue;
                m_FilteredItems.Clear();
                if (string.IsNullOrEmpty(m_SearchStr))
                {
                    m_FilteredItems.AddRange(m_SearchItems);
                }
                else
                {
                    for (int i = 0; i < m_SearchItems.Count; i++)
                    {
                        (string path, string name) tuple = m_SearchItems[i];
                        if (tuple.name.Contains(m_SearchStr, StringComparison.OrdinalIgnoreCase))
                        {
                            m_FilteredItems.Add(tuple);
                        }
                    }
                }
                m_ListView.RefreshItems();
            }
        }

        private void OnAddGraphAsset(string graphAssetPath)
        {
            string graphAssetName = Path.GetFileNameWithoutExtension(graphAssetPath);
            ValueTuple<string, string> tuple = new(graphAssetPath, graphAssetName);
            m_SearchItems.Add(tuple);
            if (string.IsNullOrEmpty(m_SearchStr) || graphAssetName.Contains(m_SearchStr, StringComparison.OrdinalIgnoreCase))
            {
                m_FilteredItems.Add(tuple);
                m_ListView.RefreshItems();
            }
        }

        private void OnRemoveGraphAsset(string graphAssetPath)
        {
            for (int i = 0; i < m_SearchItems.Count; i++)
            {
                if (m_SearchItems[i].path == graphAssetPath)
                {
                    (m_SearchItems[i], m_SearchItems[^1]) = (m_SearchItems[^1], m_SearchItems[i]);
                    m_SearchItems.RemoveAt(m_SearchItems.Count - 1);
                    break;
                }
            }
            for (int i = 0; i < m_FilteredItems.Count; i++)
            {
                if (m_FilteredItems[i].path == graphAssetPath)
                {
                    m_FilteredItems.RemoveAt(i);
                    m_ListView.RefreshItems();
                }
            }
            DestroyGraphView(graphAssetPath);
        }

        #endregion

        public GraphAssetDrawer GetCurrentGraphAssetDrawer()
        {
            return m_CurrentGraphAssetDrawer;
        }

        private void ChangeMainGraphView(string graphAssetPath)
        {
            if (m_CurrentGraphAssetPath != graphAssetPath)
            {
                GraphAssetDrawer graphAssetDrawer = null;
                if (!string.IsNullOrEmpty(graphAssetPath) && !m_GraphAssetDrawers.TryGetValue(graphAssetPath, out graphAssetDrawer))
                {
                    GraphAsset graphAsset = GraphAssetSaveProcessor.GetGraphAsset(graphAssetPath);
                    if (graphAsset == null)
                    {
                        Debug.LogError($"Graph asset at path:{graphAssetPath} could not found");
                        return;
                    }
                    graphAssetDrawer = GraphAssetDrawer.Allocate();
                    graphAssetDrawer.DrawGraphView(graphAsset);
                    m_GraphAssetDrawers.Add(graphAssetPath, graphAssetDrawer);
                }
                if (m_CurrentGraphAssetDrawer != null)
                {
                    m_GraphContainer.Remove(m_CurrentGraphAssetDrawer.GetGraphView());
                }
                if (graphAssetDrawer != null)
                {
                    CustomGraphView graphView = graphAssetDrawer.GetGraphView();
                    m_GraphContainer.Add(graphView);
                }
                m_CurrentGraphAssetPath = graphAssetPath;
                m_CurrentGraphAssetDrawer = graphAssetDrawer;
            }
        }

        public void DestroyGraphView(string graphAssetPath)
        {
            if (m_CurrentGraphAssetPath == graphAssetPath)
            {
                if (m_CurrentGraphAssetDrawer != null)
                {
                    ChangeMainGraphView(null);
                }
            }
            if (m_GraphAssetDrawers.Remove(graphAssetPath, out GraphAssetDrawer destroyTarget))
            {
                GraphAssetDrawer.Release(destroyTarget);
                CustomGraphView.Release(destroyTarget.GetGraphView());
            }
        }

        private void CreateGUI()
        {
            if (s_Instance != this)
            {
                if (s_Instance != null)
                {
                    s_Instance.Close();
                }
                s_Instance = this;
            }
            RuntimeToEditorMap.GetInstance().Initialize();
            NodeSearchEntry.InitializeNodeSearchTree();
            if (!m_HasInitializedGraphAssetName)
            {
                HashSet<string>.Enumerator allGraphAssetPaths = GraphAssetPostprocessor.GetAllGraphAssetPaths();
                while (allGraphAssetPaths.MoveNext())
                {
                    string graphAssetPath = allGraphAssetPaths.Current;
                    string graphAssetName = Path.GetFileNameWithoutExtension(graphAssetPath);
                    ValueTuple<string, string> tuple = new(graphAssetPath, graphAssetName);
                    m_SearchItems.Add(tuple);
                    m_FilteredItems.Add(tuple);
                }
                allGraphAssetPaths.Dispose();
                GraphAssetPostprocessor.OnAddGraphAsset += OnAddGraphAsset;
                GraphAssetPostprocessor.OnRemoveGraphAsset += OnRemoveGraphAsset;
                m_HasInitializedGraphAssetName = true;
            }
            //创建边栏搜索框和蓝图主视图容器
            VisualElement graphAssetView = new()
            {
                style =
                {
                    maxWidth = 300,
                    minWidth = 100,
                    flexShrink = 0,
                    flexDirection = FlexDirection.Column,
                    borderRightWidth = 1
                }
            };
            m_GraphContainer = new VisualElement
            {
                style =
                {
                    flexGrow = 1
                }
            };
            TextField searchField = new()
            {
                value = m_SearchStr
            };
            searchField.RegisterValueChangedCallback(OnSearchStrChanged);
            m_ListView = new ListView(m_FilteredItems, 20, MakeItem, BindItem);
            graphAssetView.Add(searchField);
            graphAssetView.Add(m_ListView);
            TwoPaneSplitView splitView = new(0, 200, TwoPaneSplitViewOrientation.Horizontal);
            splitView.Add(graphAssetView);
            splitView.Add(m_GraphContainer);
            rootVisualElement.Add(splitView);
        }

        private void OnDestroy()
        {
            foreach (KeyValuePair<string, GraphAssetDrawer> kvp in m_GraphAssetDrawers)
            {
                GraphAssetDrawer.Release(kvp.Value);
            }
            m_GraphAssetDrawers.Clear();
            if (s_Instance == this)
            {
                s_Instance = null;
            }
            m_HasInitializedGraphAssetName = false;
            GraphAssetPostprocessor.OnAddGraphAsset -= OnAddGraphAsset;
            GraphAssetPostprocessor.OnRemoveGraphAsset -= OnRemoveGraphAsset;
            m_CurrentGraphAssetPath = null;
            m_CurrentGraphAssetDrawer = null;
        }

        #region Single instance

        private static GraphWindow s_Instance;

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

        #endregion
    }
}