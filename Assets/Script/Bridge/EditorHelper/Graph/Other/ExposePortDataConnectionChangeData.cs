#if UNITY_EDITOR
using System.Collections.Generic;
using YBFramework.Bridge.Data;

namespace YBFramework.Bridge.Editor
{
    public static class ExposePortDataConnectionChangeData
    {
        public sealed class SubGraphConnectionChangeData
        {
            public sealed class ConnectionChangeData
            {
                private static readonly Stack<ConnectionChangeData> s_ConnectionChangeDataPool = new();

                public static ConnectionChangeData Allocate(ExposePortData exposePortData, int toExposeNodeID, int toExposePortID, bool isConnect)
                {
                    ConnectionChangeData connectionChangeData = s_ConnectionChangeDataPool.Count > 0 ? s_ConnectionChangeDataPool.Pop() : new ConnectionChangeData();
                    connectionChangeData.ExposePortData = exposePortData;
                    connectionChangeData.ToExposeNodeID = toExposeNodeID;
                    connectionChangeData.ToExposePortID = toExposePortID;
                    connectionChangeData.IsConnect = isConnect;
                    return connectionChangeData;
                }

                public static void Free(ConnectionChangeData connectionChangeData)
                {
                    s_ConnectionChangeDataPool.Push(connectionChangeData);
                }

                public ExposePortData ExposePortData;

                public int ToExposeNodeID;

                public int ToExposePortID;

                public bool IsConnect;
            }

            private static readonly Stack<SubGraphConnectionChangeData> s_Pool = new();

            public static SubGraphConnectionChangeData Allocate(GraphAsset graphAsset)
            {
                SubGraphConnectionChangeData connectionChangeData = s_Pool.Count > 0 ? s_Pool.Pop() : new SubGraphConnectionChangeData();
                connectionChangeData.GraphAsset = graphAsset;
                return connectionChangeData;
            }

            public static void Free(SubGraphConnectionChangeData connectionChangeData)
            {
                for (int i = 0; i < connectionChangeData.m_ConnectionsChangeData.Count; i++)
                {
                    ConnectionChangeData.Free(connectionChangeData.m_ConnectionsChangeData[i]);
                }
                connectionChangeData.m_ConnectionsChangeData.Clear();
                s_Pool.Push(connectionChangeData);
            }

            public GraphAsset GraphAsset;

            private readonly List<ConnectionChangeData> m_ConnectionsChangeData = new();

            public IReadOnlyList<ConnectionChangeData> GetConnectionsChangeData()
            {
                return m_ConnectionsChangeData;
            }

            public void TryAddConnectionChangeData(ExposePortData exposePortData, int toExposeNodeID, int toExposePortID, bool isConnect)
            {
                for (int i = 0; i < m_ConnectionsChangeData.Count; i++)
                {
                    ConnectionChangeData connectionChangeData = m_ConnectionsChangeData[i];
                    if (connectionChangeData.ExposePortData == exposePortData && connectionChangeData.ToExposeNodeID == toExposeNodeID && connectionChangeData.ToExposePortID == toExposePortID)
                    {
                        if (connectionChangeData.IsConnect != isConnect)
                        {
                            ConnectionChangeData.Free(connectionChangeData);
                            m_ConnectionsChangeData.RemoveAt(i);
                        }
                        return;
                    }
                }
                m_ConnectionsChangeData.Add(ConnectionChangeData.Allocate(exposePortData, toExposeNodeID, toExposePortID, isConnect));
            }

            public int GetConnectionChangeDataCount()
            {
                return m_ConnectionsChangeData.Count;
            }
        }

        /// <summary>
        /// 保存时需要进行同步的子蓝图数据
        /// 采用List而不是字典的原因：在进行数据同步时也可能出现需要添加新的子蓝图进来，List使用for可以保证执行顺序，而字典在foreach中添加或者移除会报错，就算不报错也不能保证顺序
        /// </summary>
        private static readonly List<SubGraphConnectionChangeData> s_SubGraphConnectionsChangeData = new();

        public static IReadOnlyList<SubGraphConnectionChangeData> GetSubGraphConnectionsChangeData()
        {
            return s_SubGraphConnectionsChangeData;
        }

        public static void AddConnectionChangeData(GraphAsset graphAsset, ExposePortData exposePortData, int toExposeNodeID, int toExposePortID, bool isConnect)
        {
            int index = -1;
            for (int i = 0; i < s_SubGraphConnectionsChangeData.Count; i++)
            {
                if (s_SubGraphConnectionsChangeData[i].GraphAsset == graphAsset)
                {
                    index = i;
                    break;
                }
            }
            SubGraphConnectionChangeData subGraphConnectionChangeData;
            if (index == -1)
            {
                subGraphConnectionChangeData = SubGraphConnectionChangeData.Allocate(graphAsset);
                index = s_SubGraphConnectionsChangeData.Count;
                if (s_SubGraphConnectionsChangeData.Count == 0)
                {
                    GraphAssetSaveProcessRegister.RegisterProcess("Expose port data save process");
                }
                s_SubGraphConnectionsChangeData.Add(subGraphConnectionChangeData);
            }
            else
            {
                subGraphConnectionChangeData = s_SubGraphConnectionsChangeData[index];
            }
            subGraphConnectionChangeData.TryAddConnectionChangeData(exposePortData, toExposeNodeID, toExposePortID, isConnect);
            if (subGraphConnectionChangeData.GetConnectionChangeDataCount() == 0)
            {
                s_SubGraphConnectionsChangeData.RemoveAt(index);
                if (s_SubGraphConnectionsChangeData.Count == 0)
                {
                    GraphAssetSaveProcessRegister.UnregisterProcess("Expose port data save process");
                }
            }
        }

        public static void Clear()
        {
            for (int i = 0; i < s_SubGraphConnectionsChangeData.Count; i++)
            {
                SubGraphConnectionChangeData.Free(s_SubGraphConnectionsChangeData[i]);
            }
            s_SubGraphConnectionsChangeData.Clear();
        }
    }
}
#endif