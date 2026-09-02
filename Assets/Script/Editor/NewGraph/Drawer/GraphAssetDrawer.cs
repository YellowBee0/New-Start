using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using YBFramework.Bridge.Data;

namespace YBFramework.Editor.NewGraph
{
    public sealed class GraphAssetDrawer
    {
        private GraphAsset m_GraphAsset;

        private CustomGraphView m_GraphView;

        private EdgeConnector<Edge> m_EdgeConnector;

        private SerializedObject m_SO;

        private SerializedProperty m_NodeDataListProperty;

        private readonly List<BaseNodeDrawer> m_NodeDrawers = new();

        private int m_UndoGroupIndex;

        private bool m_IsModifyGraphAsset;

        public GraphAsset GetGraphAsset()
        {
            return m_GraphAsset;
        }

        public CustomGraphView GetGraphView()
        {
            return m_GraphView;
        }

        public EdgeConnector<Edge> GetEdgeConnector()
        {
            return m_EdgeConnector;
        }

        public SerializedObject GetSO()
        {
            return m_SO;
        }

        public SerializedProperty GetNodeDataListProperty()
        {
            return m_NodeDataListProperty;
        }

        public IReadOnlyList<BaseNodeDrawer> GetNodeDrawers()
        {
            return m_NodeDrawers;
        }

        public void AddNodeDrawer(BaseNodeDrawer nodeDrawer)
        {
            m_NodeDrawers.Add(nodeDrawer);
        }

        public void RemoveNodeDrawer(BaseNodeDrawer nodeDrawer)
        {
            if (m_NodeDrawers.Remove(nodeDrawer))
            {
                nodeDrawer.ClearPortDrawers();
                BaseNodeDrawer.Release(nodeDrawer);
            }
        }

        public void ClearNodeDrawers()
        {
            for (int i = 0; i < m_NodeDrawers.Count; i++)
            {
                BaseNodeDrawer nodeDrawer = m_NodeDrawers[i];
                nodeDrawer.ClearPortDrawers();
                BaseNodeDrawer.Release(nodeDrawer);
            }
            m_NodeDrawers.Clear();
        }

        public void DrawGraphView(GraphAsset graphAsset)
        {
            m_GraphAsset = graphAsset;
            m_GraphView = new CustomGraphView();
            m_EdgeConnector = new EdgeConnector<Edge>(new EdgeConnectorListener(this));
            m_GraphView.graphViewChanged += OnGraphViewChanged;
            IReadOnlyList<BaseNodeData> nodesData = graphAsset.GetNodesData();
            for (int i = 0; i < nodesData.Count; i++)
            {
                BaseNodeData nodeData = nodesData[i];
                BaseNodeDrawer nodeDrawer = BaseNodeDrawer.Allocate(nodeData.GetType());
                if (nodeDrawer != null)
                {
                    m_GraphView.AddNodeView(nodeDrawer.DrawNodeView(this, nodeData));
                    AddNodeDrawer(nodeDrawer);
                }
            }
        }

        /// <summary>
        /// 在修改GraphAsset数据之前调用此方法，以便支持Undo，并且在修改完成后调用ApplyModifyGraphAsset方法。
        /// </summary>
        /// <param name="undoName">本次Undo名</param>
        /// <exception cref="InvalidOperationException">如果调用了一次这个方法，没调用应用修改将会抛出异常</exception>
        public void ModifyGraphAsset(string undoName)
        {
            if (m_IsModifyGraphAsset)
            {
                throw new InvalidOperationException("You have already called ModifyGraphAsset, please call ApplyModifyGraphAsset before calling ModifyGraphAsset again.");
            }
            m_IsModifyGraphAsset = true;
            Undo.IncrementCurrentGroup();
            m_UndoGroupIndex = Undo.GetCurrentGroup();
            Undo.SetCurrentGroupName(undoName);
            Undo.RegisterCompleteObjectUndo(m_GraphAsset, undoName);
        }

        /// <summary>
        /// 应用修改GraphAsset数据，并且支持Undo。
        /// </summary>
        public void ApplyModifyGraphAsset()
        {
            if (m_IsModifyGraphAsset)
            {
                //TODO:是否要区分通过SerializedObject修改还是直接内存修改？
                // 通过SerializedObject修改会自己记录Undo，我需要通过UndoGroup区分
                EditorUtility.SetDirty(m_GraphAsset);
                Undo.CollapseUndoOperations(m_UndoGroupIndex);
                m_IsModifyGraphAsset = false;
            }
        }

        private void OnEdgeConnect(Edge edge)
        {
        }

        private GraphViewChange OnGraphViewChanged(GraphViewChange changeData)
        {
            //这里需要删除changeData里面的数据
            return changeData;
        }

        #region Edge connector class

        private sealed class EdgeConnectorListener : IEdgeConnectorListener
        {
            private readonly GraphAssetDrawer m_GraphAssetDrawer;

            public EdgeConnectorListener(GraphAssetDrawer graphAssetDrawer)
            {
                m_GraphAssetDrawer = graphAssetDrawer;
            }

            public void OnDropOutsidePort(Edge edge, Vector2 position)
            {
            }

            public void OnDrop(GraphView graphView, Edge edge)
            {
                m_GraphAssetDrawer.OnEdgeConnect(edge);
            }
        }

        #endregion

        #region Pool

        private static readonly Stack<GraphAssetDrawer> s_Pool = new();

        public static GraphAssetDrawer Allocate()
        {
            return s_Pool.Count > 0 ? s_Pool.Pop() : new GraphAssetDrawer();
        }

        //TODO:调用Release的地方需要同时调用ClearNodeDrawers
        public static void Release(GraphAssetDrawer graphAssetPresenter)
        {
            s_Pool.Push(graphAssetPresenter);
        }

        #endregion
    }
}