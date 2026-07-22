using System.Collections.Generic;
using UnityEngine;
using YBFramework.GameLogic.Graph;
#if UNITY_EDITOR
using YBFramework.Bridge.Editor;
#endif

namespace YBFramework.Bridge.Data
{
    [CreateAssetMenu(fileName = "New Graph Asset", menuName = "Graph Asset")]
    public sealed class GraphAsset : ScriptableObject
    {
        [SerializeReference] private List<BaseNodeData> m_NodeData;

        public BaseNodeData GetNodeData(int nodeID)
        {
            for (int i = 0; i < m_NodeData.Count; i++)
            {
                BaseNodeData nodeData = m_NodeData[i];
                if (nodeData.NodeID == nodeID)
                {
                    return nodeData;
                }
            }
            return null;
        }

        public IReadOnlyList<BaseNodeData> GetNodeData()
        {
            return m_NodeData;
        }

        public Graph CreateGraph()
        {
            Graph graph = new();
            graph.InitializeFromGraphAsset(this);
            return graph;
        }
#if UNITY_EDITOR
        [SerializeField] private GraphType m_GraphType;

        private bool m_IsInitialized;

        public int SourceNodeID;

        public GraphType GetGraphType()
        {
            return m_GraphType;
        }

        public void Initialize()
        {
            if (m_IsInitialized)
            {
                return;
            }
            for (int i = 0; i < m_NodeData.Count; i++)
            {
                BaseNodeData nodeData = m_NodeData[i];
                nodeData.GraphAsset = this;
                nodeData.Initialize();
            }
            m_IsInitialized = true;
        }

        /// <summary>
        /// 编辑器中添加一个节点数据，在蓝图编辑器中添加一个节点数据步骤：
        /// 1、调用GraphAsset的AddNodeData，持久化数据
        /// 2、调用NodeData的CreateNodeView，创建节点视图
        /// 3、调用CustomGraphView的AddNodeView，添加节点视图
        /// 示例可参考NodeSearchEntry的OnSelectEntry实现
        /// </summary>
        /// <param name="nodeData">添加的节点数据</param>
        public void AddNodeData(BaseNodeData nodeData)
        {
            m_NodeData.Add(nodeData);
            nodeData.GraphAsset = this;
        }

        /// <summary>
        /// 编辑器中移除一个节点数据，在蓝图编辑器中添加一个节点数据步骤：
        /// 1、调用GraphAsset的RemoveNodeData，移除持久化的数据
        /// 2、调用CustomGraphView的RemoveNodeView，移除节点视图
        /// 示例可参考CustomGraphView的OnGraphViewChanged实现
        /// 但是示例里在移除一个节点时会连着节点的连线一起移除（不止视图还有持久化数据，所以这里就不需要找这个节点连接的数据然后在移除），所以这并不是完整流程
        /// </summary>
        /// <param name="nodeData">移除的节点数据</param>
        public void RemoveNodeData(BaseNodeData nodeData)
        {
            m_NodeData.Remove(nodeData);
        }

        private void OnDisable()
        {
            m_IsInitialized = false;
        }
#endif
    }
}