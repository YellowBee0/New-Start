using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using YBFramework.Bridge.Editor;
#endif

namespace YBFramework.Bridge.Data
{
    [CreateAssetMenu(menuName = "Data/GraphAsset", fileName = "NewGraphAsset")]
    public sealed class GraphAsset : ScriptableObject
    {
        [SerializeReference] private List<BaseNodeData> m_NodesData;

        public IReadOnlyList<BaseNodeData> GetNodesData()
        {
            return m_NodesData;
        }

        public BaseNodeData FindNodeData(int nodeID)
        {
            for (int i = 0; i < m_NodesData.Count; i++)
            {
                BaseNodeData nodeData = m_NodesData[i];
                if (nodeData.GetNodeID() == nodeID)
                {
                    return nodeData;
                }
            }
            return null;
        }
#if UNITY_EDITOR
        public GraphType GraphType;

        public int SourceNodeID;

        private bool m_IsInitializedReference;

        public void InitializeReference()
        {
            if (!m_IsInitializedReference)
            {
                for (int i = 0; i < m_NodesData.Count; i++)
                {
                    BaseNodeData nodeData = m_NodesData[i];
                    nodeData.SetGraphAsset(this);
                    int portDataCount = nodeData.GetPortsDataCount();
                    for (int j = 0; j < portDataCount; j++)
                    {
                        BasePortData portData = nodeData.PortDataOfIndex(j);
                        portData.SetNodeData(nodeData);
                    }
                }
            }
            m_IsInitializedReference = true;
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
            //创建节点步骤
            //1、创建NodeData内部持久化数据
            //存在持久化数据
            nodeData.InitializeSerializedData();
            //2、使用蓝图分配节点id，保证唯一，且起始id为1而不是0（因为端口连线在序列化时必然不为null，且NodeID和PortID初始值为0，为避免初始数据导致连线有问题，id就统一从1开始）
            //存在持久化数据
            nodeData.SetNodeID(++SourceNodeID);
            //初始化非序列化数据
            //不存在数据持久化
            nodeData.SetGraphAsset(this);
            int portDataCount = nodeData.GetPortsDataCount();
            for (int i = 0; i < portDataCount; i++)
            {
                BasePortData portData = nodeData.PortDataOfIndex(i);
                portData.SetNodeData(nodeData);
            }
            m_NodesData.Add(nodeData);
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
            m_NodesData.Remove(nodeData);
        }
#endif
    }
}