#if UNITY_EDITOR
using System;
using UnityEngine;
using UnityEngine.UIElements;
using YBFramework.Bridge.Editor;
using YBFramework.GameLogic.Graph;

namespace YBFramework.Bridge.Data
{
    [Serializable]
    public sealed class ProxyTargetPortData : BasePortData
    {
        //TODO:这个被创建出来时直接使用代理目标端口的name
        public string ProxyName;

        [SerializeField] private PortConnectionData m_PortConnectionData;

        public override BasePort CreateRuntimeInstance()
        {
            return null;
        }

        public override PortConnectionData GetPortConnectionDataFromSelf(int nodeId, int portId)
        {
            if (m_PortConnectionData.NodeID == nodeId && m_PortConnectionData.PortID == portId)
            {
                return m_PortConnectionData;
            }
            return null;
        }

        public override int GetPortConnectionDataCountFromSelf()
        {
            if (m_PortConnectionData.NodeID != 0 && m_PortConnectionData.PortID != 0)
            {
                return 1;
            }
            return 0;
        }

        public override BasePortData Clone()
        {
            throw new Exception("this port can not clone for proxy port");
        }

        public override VisualElement CreatePortContentView(out PortView portView)
        {
            VisualElement portContentView = base.CreatePortContentView(out portView);
            TextField proxyNameField = new()
            {
                value = ProxyName
            };
            proxyNameField.RegisterValueChangedCallback(OnProxyNameChange);
            portContentView.Add(proxyNameField);
            return portContentView;
        }

        private void OnProxyNameChange(ChangeEvent<string> evt)
        {
            ProxyName = evt.newValue;
        }
    }
}
#endif