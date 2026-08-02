using UnityEditor;
using YBFramework.Bridge.Data;

namespace YBFramework.Editor.Graph
{
    [RuntimeToEditor(typeof(ProxyPortData))]
    public sealed class ProxyPortPresenter : BasePortPresenter
    {
        private BasePortPresenter m_InternalPortPresenter;

        public override void Initialize(BasePortData portData, SerializedProperty portSerializedProperty)
        {
            m_PortData = portData;
            ProxyPortData proxyPortData = (ProxyPortData)portData;
            m_InternalPortPresenter = AllocatePortPresenter(proxyPortData.ClonedTargetPortData.GetType());
            if (m_InternalPortPresenter != null)
            {
                m_InternalPortPresenter.Initialize(proxyPortData.ClonedTargetPortData, portSerializedProperty.FindPropertyRelative(proxyPortData.ClonedTargetPortData.GetFiledName()));
                m_PortView = m_InternalPortPresenter.GetPortView();
                m_PortContentView = m_InternalPortPresenter.GetPortContentView();
            }
        }

        public override void OnRelease()
        {
            base.OnRelease();
            ReleasePortPresenter(m_InternalPortPresenter);
        }
    }
}