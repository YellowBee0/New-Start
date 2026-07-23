using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using YBFramework.Bridge.Data;

namespace YBFramework.Editor
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
        private readonly Stack<GraphDrawer> m_GraphDrawerPool = new();

        private readonly Dictionary<string, CustomGraphView> m_DrawnGraphViews = new();

        private readonly List<string> m_FilteredGraphAssetNames = new();

        private readonly List<string> m_GraphAssetNames = new();

        private readonly List<string> m_GraphAssetPaths = new();

        private ListView m_ListView;

        private VisualElement m_GraphContainer;

        private CustomGraphView m_MainGraphView;

        [NonSerialized] private string m_FilterGraphAssetNameStr;

        [NonSerialized] private string m_MainGraphAssetPath;

        public CustomGraphView GetMainGraphView()
        {
            return m_MainGraphView;
        }

        public GraphDrawer AllocateGraphDrawer()
        {
            return m_GraphDrawerPool.Count > 0 ? m_GraphDrawerPool.Pop() : new GraphDrawer();
        }

        public void ReleaseGraphDrawer(GraphDrawer graphDrawer)
        {
            m_GraphDrawerPool.Push(graphDrawer);
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
            GraphDrawerMap.GetInstance().Initialize();
            //1、初始化节点筛选窗口
            NodeSearchEntry.InitializeNodeSearchTree();

            //2、加载所有蓝图资源路径，用于边栏搜索框选择和显示。这一步需要先于步骤3，因为m_FilterGraphAssetNameStr可能不为null
            string[] guids = AssetDatabase.FindAssets($"t:{nameof(GraphAsset)}");
            for (int i = 0; i < guids.Length; i++)
            {
                string graphPath = AssetDatabase.GUIDToAssetPath(guids[i]);
                m_GraphAssetPaths.Add(graphPath);
                string graphAssetName = graphPath[(graphPath.LastIndexOf('/') + 1)..].Split('.')[0];
                m_GraphAssetNames.Add(graphAssetName);
                m_FilteredGraphAssetNames.Add(graphAssetName);
            }

            //3、创建边栏搜索框和蓝图主视图容器
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

        private void ChangeMainGraphView(string graphAssetPath)
        {
            if (m_MainGraphAssetPath != graphAssetPath)
            {
                if (!m_DrawnGraphViews.TryGetValue(graphAssetPath, out CustomGraphView graphView))
                {
                    GraphAsset graphAsset = AssetDatabase.LoadAssetAtPath<GraphAsset>(graphAssetPath);
                    if (graphAsset == null)
                    {
                        Debug.LogError($"Graph asset at path:{graphAssetPath} could not found");
                        return;
                    }
                    graphAsset.Initialize();
                    graphView = AllocateGraphDrawer().CreateGraphView(graphAsset);
                    m_DrawnGraphViews.Add(graphAssetPath, graphView);
                }
                m_MainGraphView?.RemoveFromHierarchy();
                m_GraphContainer.Add(graphView);
                m_MainGraphAssetPath = graphAssetPath;
                m_MainGraphView = graphView;
            }
        }

        //TODO:销毁GraphView视图。需要销毁的内容：GraphView
    }
}