using System;
using YBFramework.Common;
using YBFramework.GameLogic.Graph;
#if UNITY_EDITOR
using UnityEngine;
#endif

namespace YBFramework.Bridge.Data
{
    [Serializable]
    public abstract class BaseNodeData : IValueIterator<BasePortData>
    {
        public int NodeID;

        public abstract BaseNode CreateRuntimeInstance();

        /// <summary>
        /// 获取节点中所有的端口数据
        /// </summary>
        /// <param name="index">索引</param>
        /// <param name="current">当前数据</param>
        /// <returns>是否执行到下一个元素</returns>
        public abstract bool Iterator(int index, out BasePortData current);

        public BasePortData GetPortData(int portID)
        {
            foreach (BasePortData portData in (IValueIterator<BasePortData>)this)
            {
                if (portData.PortID == portID)
                {
                    return portData;
                }
            }
            return null;
        }
#if UNITY_EDITOR
        public static TPortData CreatePortData<TPortData>() where TPortData : BasePortData, new()
        {
            TPortData portData = new();
            portData.InitializeSerializedData();
            return portData;
        }

        protected GraphAsset m_GraphAsset;
        
        private bool m_IsInitialized;
        
        /// <summary>
        /// Port的计数器
        /// 添加这个计算器的原因：一个节点在代码中很有可能会修改端口，比如删除一个值输入端口，添加一个函数端口。
        /// 如果没有这个计数器，端口id可能会重复，但是节点类型却不相同，导致运行时报错
        /// </summary>
        [SerializeField] private int m_PortCounter;

        public string Name;

        public Vector2 Position;

        protected int GetNextPortID()
        {
            return ++m_PortCounter;
        }

        public GraphAsset GetGraphAsset()
        {
            return m_GraphAsset;
        }

        public void SetGraphAsset(GraphAsset graphAsset)
        {
            m_GraphAsset = graphAsset;
        }
        
        /// <summary>
        /// 初始化节点数据，初始化的数据不会持久化。
        /// 该函数总是在创建节点视图或者使用节点API之前调用
        /// </summary>
        public void Initialize()
        {
            if (!m_IsInitialized)
            {
                OnInitialize();
                m_IsInitialized = true;
            }
        }
        
        /// <summary>
        /// 创建序列化数据并初始化这些数据
        /// </summary>
        public abstract void InitializeSerializedData();
        
        protected abstract void OnInitialize();

        /// <summary>
        /// 迁移节点数据
        /// 当新增/删除端口或者其他序列化且带有初始值的数据时调用
        /// 函数在调用过一次后内部实现需要及时删除，以免出现重复迁移数据
        /// </summary>
        /// <param name="graphAsset">节点数据所在的GraphAsset，用于保存迁移的数据以便显示和查看</param>
        /// <returns>是否迁移</returns>
        public virtual bool MigrateSerializedData(GraphAsset graphAsset)
        {
            bool isMigrated = false;
            foreach (BasePortData portData in (IValueIterator<BasePortData>)this)
            {
                if (portData.MigrateSerializedData(graphAsset))
                {
                    isMigrated = true;
                }
            }
            return isMigrated;
        }
#endif
    }
}