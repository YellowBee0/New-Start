using System.Collections.Generic;
using YBFramework.Bridge;
using YBFramework.Common;

namespace YBFramework.GameLogic.Graph
{
    //TODO:可添加到对象池管理
    public sealed class Graph
    {
        private readonly List<BaseNode> m_Nodes = new();

        private Entity m_Owner;

        public void InitializeFromGraphAsset(GraphAsset graphAsset)
        {
            IReadOnlyList<BaseNodeData> nodeData = graphAsset.GetNodeData();
            for (int i = 0; i < nodeData.Count; i++)
            {
                BaseNode node = nodeData[i].CreateRuntimeInstance();
                if (node != null)
                {
                    m_Nodes.Add(node);
                }
            }
            for (int i = 0; i < m_Nodes.Count; i++)
            {
                BaseNode fromNode = m_Nodes[i];
                //找到对应的NodeData，获取有用的端口数据，正常情况查找的NodeData不可能为null，因为Node的id就是通过NodeData复制过来的
                //一次查找
                BaseNodeData data = graphAsset.GetNodeData(fromNode.GetNodeID());
                //轮询节点中参与连线的端口数据
                foreach (BasePortData portData in (IValueIterator<BasePortData>)data)
                {
                    //两次查找
                    BasePort fromPort = fromNode.GetPort(portData.PortID);
                    //通过端口数据连接运行时
                    foreach (PortConnectionData portConnectionData in (IValueIterator<PortConnectionData>)portData)
                    {
                        //三、四次查找
                        BasePort toPort = GetNode(portConnectionData.NodeID).GetPort(portConnectionData.PortID);
                        fromPort.ConnectPort(portConnectionData, toPort.GetActualToConnectPort());
                    }
                }
            }
        }

        public BaseNode GetNode(int nodeID)
        {
            for (int i = 0; i < m_Nodes.Count; i++)
            {
                BaseNode node = m_Nodes[i];
                if (node.GetNodeID() == nodeID)
                {
                    return node;
                }
            }
            return null;
        }

        public void SetOwner(Entity entity)
        {
            m_Owner = entity;
        }

        public Entity GetOwner()
        {
            return m_Owner;
        }

        public void Start()
        {
            for (int i = 0; i < m_Nodes.Count; i++)
            {
                m_Nodes[i].OnStart();
            }
        }

        public void Stop()
        {
            for (int i = 0; i < m_Nodes.Count; i++)
            {
                m_Nodes[i].OnStop();
            }
        }

        public void Reset()
        {
            for (int i = 0; i < m_Nodes.Count; i++)
            {
                m_Nodes[i].OnReset();
            }
        }
    }
}