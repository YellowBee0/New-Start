using System.Collections.Generic;
using YBFramework.Bridge;
using YBFramework.Common;

namespace YBFramework.Component
{
    public sealed class GraphManager : IComponent
    {
        private static readonly int s_SourceID = TrackTargetManager.GenerateSourceID();

        private readonly HashSet<Graph> m_Graphs = new();

        private Entity m_Owner;

        private int m_TrackID;

        public void InitializeFromData(GraphManagerData data)
        {
            IReadOnlyList<GraphAsset> graphAssets = data.GetGraphAssets();
            for (int i = 0; i < graphAssets.Count; i++)
            {
                m_Graphs.Add(graphAssets[i].CreateGraph());
            }
        }

        public void AddGraph(Graph graph)
        {
            m_Graphs.Add(graph);
        }

        public void RemoveGraph(Graph graph)
        {
            m_Graphs.Remove(graph);
        }
        
        public int GetTrackID()
        {
            return m_TrackID;
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