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
                for (int j = 0; j < nodeData.Count; j++)
                {
                    BaseNodeData data = nodeData[j];
                    if (data.NodeID == fromNode.GetNodeID())
                    {
                        foreach (BasePortData portData in (IValueIterator<BasePortData>)data)
                        {
                            BasePort fromPort = fromNode.GetPort(portData.PortID);
                            foreach (PortConnectionData portConnectionData in (IValueIterator<PortConnectionData>)portData)
                            {
                                BasePort toPort = GetNode(portConnectionData.NodeID).GetPort(portConnectionData.PortID);
                                fromPort.ConnectPort(portConnectionData, toPort.GetActualPortToConnect());
                            }
                        }
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