using YBFramework.Bridge.Data;

namespace YBFramework.GameLogic.Graph
{
    //子端口还是需要创建运行时对象，这个对象保存在自己蓝图中port id用于索引，内部保存的端口引用是子蓝图的实际端口引用
    public sealed class SubPort : BasePort
    {
        private BasePort m_AsSubPort;

        public void SetAsSubPort(BasePort asSubPort)
        {
            m_AsSubPort = asSubPort;
        }

        public override void MergeData(BasePortData dataToMerge)
        {
            m_AsSubPort.MergeData(dataToMerge);
        }
    }
}