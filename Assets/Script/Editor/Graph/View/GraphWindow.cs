using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using YBFramework.Bridge.Data;
using YBFramework.Editor.Graph.Presenter;

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

        private static readonly List<string> s_GraphAssetPaths = new();
        
        private readonly Dictionary<string, GraphPresenter> m_LoadGraphPresenters = new();

        private readonly List<string> m_FilteredGraphAssetNames = new();

        private readonly List<string> m_GraphAssetNames = new();

        private readonly List<string> m_GraphAssetPaths = new();

        [NonSerialized] private string m_FilterGraphAssetNameStr;

        [NonSerialized] private string m_OpenedGraphAssetPath;

        private GraphPresenter m_OpenedPresenter;

        private ListView m_ListView;

        private VisualElement m_GraphContainer;

        //TODO:蓝图资源更新逻辑
        private static void InitializeGraphAssetPaths()
        {
            string[] guids = AssetDatabase.FindAssets($"t:{nameof(GraphAsset)}");
            for (int i = 0; i < guids.Length; i++)
            {
                string graphPath = AssetDatabase.GUIDToAssetPath(guids[i]);
                s_GraphAssetPaths.Add(graphPath);
            }
        }

        [MenuItem("Tools/Migrate Graph Assets")]
        public static void MigrateSerializedData()
        {
            for (int i = 0; i < s_GraphAssetPaths.Count; i++)
            {
                string graphAssetPath = s_GraphAssetPaths[i];
                GraphAsset graphAsset = AssetDatabase.LoadAssetAtPath<GraphAsset>(graphAssetPath);
                if (graphAsset != null)
                {
                    graphAsset.InitializeReference();
                    IReadOnlyList<BaseNodeData> nodesData = graphAsset.GetNodesData();
                    bool isDirty = false;
                    for (int j = 0; j < nodesData.Count; j++)
                    {
                        if (nodesData[i].MigrateSerializedData(graphAsset))
                        {
                            isDirty = true;
                        }
                    }
                    if (isDirty)
                    {
                        EditorUtility.SetDirty(graphAsset);
                    }
                }
            }
            AssetDatabase.SaveAssets();
        }

        public static void MigrateProxyNodeSerializedData(ProxyHelperNodeData proxyHelperNodeData)
        {
            GraphAsset proxyGraphAsset = proxyHelperNodeData.GetGraphAsset();
            for (int i = 0; i < s_GraphAssetPaths.Count; i++)
            {
                string graphAssetPath = s_GraphAssetPaths[i];
                GraphAsset graphAsset = AssetDatabase.LoadAssetAtPath<GraphAsset>(graphAssetPath);
                if (graphAsset != null)
                {
                    IReadOnlyList<BaseNodeData> nodesData = graphAsset.GetNodesData();
                    bool isDirty = false;
                    for (int j = 0; j < nodesData.Count; j++)
                    {
                        if (nodesData[i] is ProxyNodeData proxyNodeData && proxyNodeData.GetProxyGraphAsset() == proxyGraphAsset)
                        {
                            //在这里迁移代理数据
                        }
                    }
                    if (isDirty)
                    {
                        EditorUtility.SetDirty(graphAsset);
                    }
                }
            }
            AssetDatabase.SaveAssets();
        }

        public GraphPresenter GetOpenedPresenter()
        {
            return m_OpenedPresenter;
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
            if (m_OpenedGraphAssetPath != graphAssetPath)
            {
                if (!m_LoadGraphPresenters.TryGetValue(graphAssetPath, out GraphPresenter graphPresenter))
                {
                    GraphAsset graphAsset = AssetDatabase.LoadAssetAtPath<GraphAsset>(graphAssetPath);
                    if (graphAsset == null)
                    {
                        Debug.LogError($"Graph asset at path:{graphAssetPath} could not found");
                        return;
                    }
                    graphAsset.InitializeReference();
                    graphPresenter = GraphPresenter.AllocateGraphPresenter();
                    graphPresenter.Initialize(graphAsset);
                    m_LoadGraphPresenters.Add(graphAssetPath, graphPresenter);
                }
                m_OpenedPresenter?.GetGraphView().RemoveFromHierarchy();
                CustomGraphView graphView = graphPresenter.GetGraphView();
                m_GraphContainer.Add(graphView);
                m_OpenedGraphAssetPath = graphAssetPath;
                m_OpenedPresenter = graphPresenter;
            }
        }

        //TODO:销毁GraphView视图。需要销毁的内容：GraphView
    }
}