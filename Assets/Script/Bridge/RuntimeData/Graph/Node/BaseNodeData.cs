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
        public static TPortData CreatePortData<TPortData>(int portID) where TPortData : BasePortData, new()
        {
            TPortData portData = new TPortData
            {
                PortID = portID
            };
            portData.CreateData();
            return portData;
        }
        
        protected GraphAsset m_GraphAsset;

        public string Name;

        public Vector2 Position;

        //public int SourcePortID;

        public GraphAsset GetGraphAsset()
        {
            return m_GraphAsset;
        }

        public void SetGraphAsset(GraphAsset graphAsset)
        {
            m_GraphAsset = graphAsset;
        }

        public abstract void CreateData();

        /// <summary>
        /// 初始化节点数据，初始化的数据不会持久化。
        /// 重写Initialize必须在第一行写base.Initialize，这段会设置每个端口的所有节点
        /// 该函数总是在创建节点视图或者使用节点API之前调用
        /// </summary>
        public virtual void Initialize()
        {
            foreach (BasePortData portData in (IValueIterator<BasePortData>)this)
            {
                portData.SetNodeData(this);
            }
        }
#endif
    }
}