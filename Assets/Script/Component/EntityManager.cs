using System.Collections.Generic;
using YBFramework.Common;

namespace YBFramework.Component
{
    public static class EntityManager
    {
        private static readonly HashSet<Entity> s_Entities = new();

        private static readonly Dictionary<Entity, List<IUpdateHandler>> s_ManagedUpdateHandlers = new();

        private static readonly Dictionary<Entity, List<IActiveHandler>> s_ManagedActiveHandlers = new();

        private static readonly List<List<IUpdateHandler>> s_RunningUpdateHandlers = new();

        public static readonly int SourceID = TrackTargetManager.GenerateSourceID();

        public static void RegisterEntity(Entity entity)
        {
            s_Entities.Add(entity);
        }

        public static void UnregisterEntity(Entity entity)
        {
            s_Entities.Remove(entity);
        }

        public static void RegisterUpdateHandler(Entity entity, IUpdateHandler handler)
        {
            if (s_Entities.Contains(entity))
            {
                if (!s_ManagedUpdateHandlers.TryGetValue(entity, out List<IUpdateHandler> handlers))
                {
                    handlers = new List<IUpdateHandler>();
                    s_ManagedUpdateHandlers.Add(entity, handlers);
                }
                handlers.Add(handler);
            }
        }

        public static void UnregisterUpdateHandler(Entity entity, IUpdateHandler handler)
        {
            if (s_ManagedUpdateHandlers.TryGetValue(entity, out List<IUpdateHandler> handlers))
            {
                handlers.Remove(handler);
            }
        }

        public static void RegisterActiveHandler(Entity entity, IActiveHandler handler)
        {
            if (s_Entities.Contains(entity))
            {
                if (!s_ManagedActiveHandlers.TryGetValue(entity, out List<IActiveHandler> handlers))
                {
                    handlers = new List<IActiveHandler>();
                    s_ManagedActiveHandlers.Add(entity, handlers);
                }
                handlers.Add(handler);
            }
        }

        public static void UnregisterActiveHandler(Entity entity, IActiveHandler handler)
        {
            if (s_ManagedActiveHandlers.TryGetValue(entity, out List<IActiveHandler> handlers))
            {
                handlers.Remove(handler);
            }
        }

        public static void SetActive(Entity entity, bool isActive)
        {
            if (s_ManagedActiveHandlers.TryGetValue(entity, out List<IActiveHandler> handlers))
            {
                foreach (IActiveHandler handler in handlers)
                {
                    if (isActive)
                    {
                        handler.OnActivate();
                    }
                    else
                    {
                        handler.OnDeactivate();
                    }
                }
                if (s_ManagedUpdateHandlers.TryGetValue(entity, out List<IUpdateHandler> updateHandlers))
                {
                    if (isActive)
                    {
                        s_RunningUpdateHandlers.Add(updateHandlers);
                    }
                    else
                    {
                        s_RunningUpdateHandlers.Remove(updateHandlers);
                    }
                }
            }
        }

        public static void Update()
        {
            for (int i = 0; i < s_RunningUpdateHandlers.Count; i++)
            {
                List<IUpdateHandler> updateHandlers = s_RunningUpdateHandlers[i];
                for (int j = 0; j < updateHandlers.Count; j++)
                {
                    updateHandlers[j].OnUpdate();
                }
            }
        }
    }
}