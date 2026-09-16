using System.Collections.Generic;
using YBFramework.Bridge.Data;

namespace YBFramework.GameLogic.Graph
{
    //TODO:可添加到对象池管理
    public sealed class Graph
    {
        private readonly List<BaseNode> m_Nodes = new();

        //TODO:owner目前想到的办法就是使用object表示，因为蓝图存在的所有者可能不止Entity，现在已经有在Buff中的设定了，以后可能会存在于UI。
        // 需要一个类GetGraphOwner<T>，T为所有者类型Entity、Buff。。。这个节点为泛型节点需要手动添加搜索列表，区分Entity、Buff。。。
        // 创建的类型分别为GetGraphOwner<Entity>、GetGraphOwner<Buff>。。。名称可以不用区分
        //这个值先于节点和端口的创建，否则子图获取不到owner的值
        private object m_Owner;

        public void InitializeFromGraphAsset(GraphAsset graphAsset)
        {
            /*IReadOnlyList<BaseNodeData> nodeData = graphAsset.GetNodesData();
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
                    if (portData.IsUsed)
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
            }*/
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

        public void SetOwner(object owner)
        {
            m_Owner = owner;
        }

        public object GetOwner()
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