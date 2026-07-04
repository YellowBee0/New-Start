using System;
using YBFramework.Bridge;
using YBFramework.Common;

namespace YBFramework.Component
{
    public sealed class Graph : IComponent
    {
        private static readonly int s_SourceID = TrackTargetManager.GenerateSourceID();

        private Entity m_Owner;

        private int m_TrackID;

        public int GetTrackID()
        {
            return m_TrackID;
        }

        public void InitializeFromData(IComponentData data)
        {
            throw new NotImplementedException();
        }

        public Entity GetOwner()
        {
            return m_Owner;
        }

        public void SetOwner(Entity entity)
        {
            m_Owner = entity;
            m_TrackID = TrackTargetManager.CombineTrackID(entity.GetTrackID(), s_SourceID);
        }

        public void OnAdd()
        {
            TrackTargetManager.NotifyGetControl(m_TrackID);
        }

        public void OnRemove()
        {
            TrackTargetManager.NotifyLoseControl(m_TrackID);
        }
    }
}