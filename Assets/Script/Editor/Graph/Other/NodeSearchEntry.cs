using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using YBFramework.Bridge.Data;
using YBFramework.Bridge.Editor;
using YBFramework.Editor.Graph.Presenter;

namespace YBFramework.Editor.Graph
{
    public sealed class NodeSearchEntry : ScriptableObject, ISearchWindowProvider
    {
        #region Node search tree initialize
        private sealed class NodeMenuBranch : NodeMenuOption
        {
            private readonly List<NodeMenuOption> m_Options = new();

            public NodeMenuBranch(string name, int level) : base(name, level)
            {
            }

            public void AddOption(NodeMenuOption option)
            {
                m_Options.Add(option);
            }

            public IReadOnlyList<NodeMenuOption> GetOptions()
            {
                return m_Options;
            }
        }

        private sealed class NodeMenuLeaf : NodeMenuOption
        {
            public readonly GraphType GraphType;

            public NodeMenuLeaf(GraphType graphType, string name, int level) : base(name, level)
            {
                GraphType = graphType;
            }
        }

        private abstract class NodeMenuOption
        {
            public readonly int Level;

            public readonly string OptionText;

            protected NodeMenuOption(string name, int level)
            {
                OptionText = name;
                Level = level;
            }
        }

        private static readonly NodeMenuBranch s_Root = new("Root", 0);

        private static readonly Dictionary<string, (Type, IEnumerable<NodeCreateLimitAttribute>)> s_NodeMetaData = new();

        private static readonly Dictionary<GraphType, NodeSearchEntry> s_NodeSearchEntries = new();

        public static void InitializeNodeSearchTree()
        {
            if (s_NodeMetaData.Count > 0)
            {
                Debug.Log("node search tree has initialized");
                return;
            }
            TypeCache.TypeCollection types = TypeCache.GetTypesDerivedFrom<BaseNodeData>("Assembly-CSharp");
            foreach (Type type in types)
            {
                NodeMenuAttribute nodeMenuAttribute = type.GetCustomAttribute<NodeMenuAttribute>();
                if (nodeMenuAttribute != null)
                {
                    CreateNodeSearchMenu(type, nodeMenuAttribute.NodeName, nodeMenuAttribute.GraphType);
                }
                else
                {
                    Debug.LogWarning($"node type {type} was missing NodeMenuAttribute");
                }
            }
        }

        public static NodeSearchEntry GetSearchEntry(GraphType graphType)
        {
            if (!s_NodeSearchEntries.TryGetValue(graphType, out NodeSearchEntry nodeSearchEntry))
            {
                nodeSearchEntry = CreateInstance<NodeSearchEntry>();
                List<SearchTreeEntry> results = new()
                {
                    new SearchTreeGroupEntry(new GUIContent(s_Root.OptionText), s_Root.Level)
                };
                RecursionSetSearchTreeEntry(s_Root, graphType, results);
                nodeSearchEntry.m_SearchTreeEntries = results;
                s_NodeSearchEntries.Add(graphType, nodeSearchEntry);
            }
            return nodeSearchEntry;
        }

        //TODO:开放一个创建节点搜索框的接口，可以自定义节点搜索内容，且创建节点也可以自定义。目前准备使用委托的方式创建
        //TODO:这个是否可以开放出去，让同一个类型得节点可以有多个选项，即筛选框得路径不同，选项名字不同
        private static void CreateNodeSearchMenu(Type nodeType, string nodeMenuPath, GraphType graphType)
        {
            ReadOnlySpan<char> menuSpan = nodeMenuPath.AsSpan();
            NodeMenuBranch curBranch = s_Root;
            string optionText;
            int startIndex = 0;
            int sliceLenght = 0;
            int level = 0;
            IReadOnlyList<NodeMenuOption> options;
            for (int i = 0; i < menuSpan.Length; i++)
            {
                if (menuSpan[i] == '/')
                {
                    level++;
                    optionText = menuSpan.Slice(startIndex, sliceLenght).ToString();
                    NodeMenuOption optionTemp = null;
                    options = curBranch.GetOptions();
                    for (int j = 0; j < options.Count; j++)
                    {
                        if (options[j].OptionText == optionText && options[j] is NodeMenuBranch)
                        {
                            optionTemp = options[j];
                            break;
                        }
                    }
                    if (optionTemp == null)
                    {
                        optionTemp = new NodeMenuBranch(optionText, level);
                        curBranch.AddOption(optionTemp);
                    }
                    curBranch = (optionTemp as NodeMenuBranch)!;
                    startIndex = i + 1;
                    sliceLenght = 0;
                }
                else
                {
                    sliceLenght++;
                }
            }
            optionText = $"{menuSpan[startIndex..].ToString()}({graphType})";
            options = curBranch.GetOptions();
            for (int i = 0; i < options.Count; i++)
            {
                if (options[i].OptionText == optionText)
                {
                    if (options[i] is NodeMenuLeaf)
                    {
                        Debug.LogWarning($"you may have the same node menu {optionText}");
                        return;
                    }
                }
            }
            curBranch.AddOption(new NodeMenuLeaf(graphType, optionText, ++level));
            IEnumerable<NodeCreateLimitAttribute> nodeCreateLimitAttributes = nodeType.GetCustomAttributes<NodeCreateLimitAttribute>();
            s_NodeMetaData.Add(optionText, (nodeType, nodeCreateLimitAttributes));
        }

        private static (Type, IEnumerable<NodeCreateLimitAttribute>) GetSelectNodeMetaData(string menuOption)
        {
            return s_NodeMetaData.GetValueOrDefault(menuOption);
        }

        private static bool RecursionSetSearchTreeEntry(NodeMenuBranch branch, GraphType graphType, in List<SearchTreeEntry> results)
        {
            bool hasAddToResult = false;
            IReadOnlyList<NodeMenuOption> options = branch.GetOptions();
            for (int i = 0; i < options.Count; i++)
            {
                if (options[i] is NodeMenuLeaf menuLeaf)
                {
                    if ((menuLeaf.GraphType & graphType) != 0)
                    {
                        results.Add(new SearchTreeEntry(new GUIContent(menuLeaf.OptionText))
                        {
                            level = menuLeaf.Level
                        });
                        hasAddToResult = true;
                    }
                }
                else
                {
                    results.Add(new SearchTreeGroupEntry(new GUIContent(options[i].OptionText), options[i].Level));
                    if (RecursionSetSearchTreeEntry(options[i] as NodeMenuBranch, graphType, in results))
                    {
                        hasAddToResult = true;
                    }
                    else
                    {
                        results.RemoveAt(results.Count - 1);
                    }
                }
            }
            return hasAddToResult;
        }
        #endregion
        private List<SearchTreeEntry> m_SearchTreeEntries;

        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
        {
            return m_SearchTreeEntries;
        }

        public bool OnSelectEntry(SearchTreeEntry SearchTreeEntry, SearchWindowContext context)
        {
            GraphPresenter openedGraphPresenter = GraphWindow.GetInstance().GetOpenedPresenter();
            if (openedGraphPresenter != null)
            {
                (Type nodeType, IEnumerable<NodeCreateLimitAttribute> createLimits) nodeMetaData = GetSelectNodeMetaData(SearchTreeEntry.name);
                if (nodeMetaData.nodeType != null)
                {
                    foreach (NodeCreateLimitAttribute nodeCreateLimitAttribute in nodeMetaData.createLimits)
                    {
                        if (!nodeCreateLimitAttribute.CanCreate(openedGraphPresenter.GetGraphAsset(), nodeMetaData.nodeType))
                        {
                            return false;
                        }
                    }
                    BaseNodePresenter nodePresenter = BaseNodePresenter.AllocateNodePresenter(nodeMetaData.nodeType);
                    if (nodePresenter != null)
                    {
                        if (Activator.CreateInstance(nodeMetaData.nodeType) is BaseNodeData nodeData)
                        {
                            //TODO:需要支持Undo
                            CustomGraphView graphView = openedGraphPresenter.GetGraphView();
                            VisualElement rootVisualElement = GraphWindow.GetInstance().rootVisualElement;
                            Vector2 worldPos = rootVisualElement.ChangeCoordinatesTo(rootVisualElement.parent, context.screenMousePosition - GraphWindow.GetInstance().position.position);
                            //添加数据
                            //存在持久化数据
                            openedGraphPresenter.GetGraphAsset().AddNodeData(nodeData);
                            //创建节点时初始话节点名和位置
                            //存在持久化数据
                            nodeData.Name = SearchTreeEntry.name;
                            nodeData.Position = graphView.contentViewContainer.WorldToLocal(worldPos);
                            //更新SO，保证能够拿到SerializedProperty
                            openedGraphPresenter.UpdateSO();
                            //初始化节点视图
                            nodePresenter.Initialize(nodeData, openedGraphPresenter.GetNodeSerializedProperty(nodeData));
                            openedGraphPresenter.AddNodePresenter(nodePresenter);
                            return true;
                        }
                    }
                }
            }
            return false;
        }
    }
}