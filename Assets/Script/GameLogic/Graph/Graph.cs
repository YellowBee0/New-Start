using System.Collections.Generic;

namespace YBFramework.GameLogic.Graph
{
    //TODO:可添加到对象池管理
    public sealed class Graph
    {
        private List<BaseNode> m_Nodes;

        //TODO:owner目前想到的办法就是使用object表示，因为蓝图存在的所有者可能不止Entity，现在已经有在Buff中的设定了，以后可能会存在于UI。
        // 需要一个类GetGraphOwner<T>，T为所有者类型Entity、Buff。。。这个节点为泛型节点需要手动添加搜索列表，区分Entity、Buff。。。
        // 创建的类型分别为GetGraphOwner<Entity>、GetGraphOwner<Buff>。。。名称可以不用区分
        //这个值先于节点和端口的创建，否则子图获取不到owner的值
        private object m_Owner;

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