using System;
using UnityEditor.Experimental.GraphView;
using YBFramework.Common;
using YBFramework.GameLogic.Graph;
#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.UIElements;
using YBFramework.Bridge.Editor;
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
        protected GraphAsset m_GraphAsset;
        
        public string Name;

        public Vector2 Position;

        public int SourcePortID;

        public void SetGraphAsset(GraphAsset graphAsset)
        {
            m_GraphAsset = graphAsset;
        }

        /// <summary>
        /// 初始化节点数据，初始化的数据不会持久化。该函数总是在创建节点视图或者使用节点API之前调用
        /// </summary>
        public virtual void Initialize()
        {
            foreach (BasePortData portData in (IValueIterator<BasePortData>)this)
            {
                portData.SetNodeData(this);
            }
        }

        public virtual NodeView CreateNodeView()
        {
            NodeView nodeView = new(this);
            foreach (BasePortData portData in (IValueIterator<BasePortData>)this)
            {
                VisualElement visualElement = portData.CreatePortContentView(out PortView portView);
                PortViewArgs portViewArgs = portData.GetPortViewArgs();
                if (portViewArgs.Direction == Direction.Input)
                {
                    nodeView.inputContainer.Add(visualElement);
                }
                else
                {
                    nodeView.outputContainer.Add(visualElement);
                }
                nodeView.Add(portView);
            }
            nodeView.RefreshPortContainerDisplay();
            return nodeView;
        }
#endif
    }
}