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
        #region single instance

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

        private readonly Dictionary<string, GraphAssetPresenter> m_LoadedGraphAssetPresenters = new();

        private readonly List<string> m_GraphAssetPaths = new();

        private readonly List<string> m_GraphAssetNames = new();

        private readonly List<string> m_FilteredGraphAssetNames = new();

        [NonSerialized] private bool m_HasInitializedGraphAssetName;

        [NonSerialized] private string m_FilterGraphAssetNameStr;

        [NonSerialized] private string m_OpenedGraphAssetPath;

        private GraphAssetPresenter m_OpenedAssetPresenter;

        private ListView m_ListView;

        private VisualElement m_GraphContainer;

        public GraphAssetPresenter GetOpenedPresenter()
        {
            return m_OpenedAssetPresenter;
        }

        private void OnSearchGraphAssetNameStrChanged(ChangeEvent<string> evt)
        {
            if (m_FilterGraphAssetNameStr != evt.newValue)
            {
                m_FilterGraphAssetNameStr = evt.newValue;
                m_FilteredGraphAssetNames.Clear();
                if (string.IsNullOrEmpty(m_FilterGraphAssetNameStr))
                {
                    m_FilteredGraphAssetNames.AddRange(m_GraphAssetNames);
                }
                else
                {
                    for (int i = 0; i < m_GraphAssetNames.Count; i++)
                    {
                        string graphAssetName = m_GraphAssetNames[i];
                        if (graphAssetName.Contains(m_FilterGraphAssetNameStr, StringComparison.OrdinalIgnoreCase))
                        {
                            m_FilteredGraphAssetNames.Add(graphAssetName);
                        }
                    }
                }
                m_ListView.RefreshItems();
            }
        }

        private void OnAddGraphAsset(string graphAssetPath)
        {
            string graphAssetName = Path.GetFileNameWithoutExtension(graphAssetPath);
            m_GraphAssetPaths.Add(graphAssetPath);
            m_GraphAssetNames.Add(graphAssetName);
            if (string.IsNullOrEmpty(m_FilterGraphAssetNameStr) || m_FilterGraphAssetNameStr.Contains(graphAssetName, StringComparison.OrdinalIgnoreCase))
            {
                m_FilteredGraphAssetNames.Add(graphAssetName);
                m_ListView.RefreshItems();
            }
        }

        private void OnRemoveGraphAsset(string graphAssetPath)
        {
            for (int i = 0; i < m_GraphAssetPaths.Count; i++)
            {
                if (m_GraphAssetPaths[i] == graphAssetPath)
                {
                    (m_GraphAssetPaths[i], m_GraphAssetPaths[^1]) = (m_GraphAssetPaths[^1], m_GraphAssetPaths[i]);
                    (m_GraphAssetNames[i], m_GraphAssetNames[^1]) = (m_GraphAssetNames[^1], m_GraphAssetNames[i]);
                    m_GraphAssetPaths.RemoveAt(m_GraphAssetPaths.Count - 1);
                    m_GraphAssetNames.RemoveAt(m_GraphAssetNames.Count - 1);
                    break;
                }
            }
            string graphAssetName = Path.GetFileNameWithoutExtension(graphAssetPath);
            if (m_FilteredGraphAssetNames.Remove(graphAssetName))
            {
                m_ListView.RefreshItems();
            }
            DestroyGraphView(graphAssetPath);
        }

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
                string graphName = m_FilteredGraphAssetNames[index];
                label.text = graphName;
                label.userData = m_GraphAssetPaths[m_GraphAssetNames.IndexOf(graphName)];
            }
        }

        #endregion
        
        private void ChangeMainGraphView(string graphAssetPath)
        {
            if (m_OpenedGraphAssetPath != graphAssetPath)
            {
                GraphAssetPresenter graphPresenter = null;
                if (!string.IsNullOrEmpty(graphAssetPath) && !m_LoadedGraphAssetPresenters.TryGetValue(graphAssetPath, out graphPresenter))
                {
                    GraphAsset graphAsset = AssetDatabase.LoadAssetAtPath<GraphAsset>(graphAssetPath);
                    if (graphAsset == null)
                    {
                        Debug.LogError($"Graph asset at path:{graphAssetPath} could not found");
                        return;
                    }
                    graphPresenter = GraphAssetPresenter.AllocateGraphAssetPresenter();
                    graphPresenter.Initialize(graphAsset);
                    m_LoadedGraphAssetPresenters.Add(graphAssetPath, graphPresenter);
                }
                m_OpenedAssetPresenter?.GetGraphView().RemoveFromHierarchy();
                if (graphPresenter != null)
                {
                    CustomGraphView graphView = graphPresenter.GetGraphView();
                    m_GraphContainer.Add(graphView);
                }
                m_OpenedGraphAssetPath = graphAssetPath;
                m_OpenedAssetPresenter = graphPresenter;
            }
        }

        public void DestroyGraphView(string graphAssetPath)
        {
            if (m_OpenedGraphAssetPath == graphAssetPath)
            {
                if (m_OpenedAssetPresenter != null)
                {
                    ChangeMainGraphView(null);
                }
            }
            if (m_LoadedGraphAssetPresenters.Remove(graphAssetPath, out GraphAssetPresenter destroyTarget))
            {
                GraphAssetPresenter.ReleaseGraphAssetPresenter(destroyTarget);
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
                    m_GraphAssetPaths.Add(graphAssetPath);
                    m_GraphAssetNames.Add(graphAssetName);
                    m_FilteredGraphAssetNames.Add(graphAssetName);
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
                value = m_FilterGraphAssetNameStr
            };
            searchField.RegisterValueChangedCallback(OnSearchGraphAssetNameStrChanged);
            m_ListView = new ListView(m_FilteredGraphAssetNames, 20, MakeItem, BindItem);
            graphAssetView.Add(searchField);
            graphAssetView.Add(m_ListView);
            TwoPaneSplitView splitView = new(0, 200, TwoPaneSplitViewOrientation.Horizontal);
            splitView.Add(graphAssetView);
            splitView.Add(m_GraphContainer);
            rootVisualElement.Add(splitView);
        }

        private void OnDestroy()
        {
            foreach (KeyValuePair<string,GraphAssetPresenter> kvp in m_LoadedGraphAssetPresenters)
            {
                GraphAssetPresenter.ReleaseGraphAssetPresenter(kvp.Value);
            }
            m_LoadedGraphAssetPresenters.Clear();
            if (s_Instance == this)
            {
                s_Instance = null;
            }
            m_HasInitializedGraphAssetName = false;
            GraphAssetPostprocessor.OnAddGraphAsset -= OnAddGraphAsset;
            GraphAssetPostprocessor.OnRemoveGraphAsset -= OnRemoveGraphAsset;
        }
    }
}