using UnityEngine.UIElements;
using YBFramework.Bridge.Data;

namespace YBFramework.Editor.Graph
{
    [RuntimeToEditor(typeof(SubPortData))]
    public sealed class SubPortDrawer : BasePortDrawer
    {
        private SubPortData m_SubPortData;

        private BasePortDrawer m_SubPortDrawer;

        public override BasePortData GetPortData()
        {
            return m_SubPortData;
        }

        public override VisualElement GetPortContentView()
        {
            return m_SubPortDrawer.GetPortContentView();
        }

        public override PortView GetPortView()
        {
            return m_SubPortDrawer.GetPortView();
        }

        protected override PortView OnDrawPortView(BasePortData portData)
        {
            SubPortData subPortData = (SubPortData)portData;
            BasePortData asSubPortData = subPortData.GetAsSubPortData();
            m_SubPortDrawer = Allocate(asSubPortData.GetType());
            if (m_SubPortDrawer != null)
            {
                m_SubPortDrawer.DrawPortView(GetNodeDrawer(), asSubPortData);
                return m_SubPortDrawer.GetPortView();
            }
            return null;
        }

        public override void OnRelease()
        {
            m_SubPortDrawer.OnRelease();
        }
    }
}