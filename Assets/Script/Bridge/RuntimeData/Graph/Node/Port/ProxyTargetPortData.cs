#if UNITY_EDITOR
using System;
using YBFramework.GameLogic.Graph;

namespace YBFramework.Bridge.Data
{
    [Serializable]
    public sealed class ProxyTargetPortData : BasePortData
    {
        //TODO:这个被创建出来时直接使用代理目标端口的name
        public string ProxyName;

        //TODO:把Data里所有序列化的数据设置为public
        public PortConnectionData PortConnectionData;

        public override BasePort CreateRuntimeInstance()
        {
            return null;
        }

        public override PortConnectionData GetPortConnectionDataFromSelf(int nodeId, int portId)
        {
            if (PortConnectionData.NodeID == nodeId && PortConnectionData.PortID == portId)
            {
                return PortConnectionData;
            }
            return null;
        }

        public override int GetPortConnectionDataCountFromSelf()
        {
            if (PortConnectionData.NodeID != 0 && PortConnectionData.PortID != 0)
            {
                return 1;
            }
            return 0;
        }

        public override BasePortData Clone()
        {
            throw new Exception("this port can not clone for proxy port");
        }
    }
}
#endif