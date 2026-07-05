using System.Collections.Generic;

namespace YBFramework.Component
{
    public static class EntityManager
    {
        private sealed class UpdateHandler
        {
            public IUpdate Update;

            public int Index;
        }
        
        private static readonly Dictionary<Entity, HashSet<IUpdate>> s_UpdateHandlers = new();

        private static readonly Dictionary<Entity, HashSet<IActivable>> s_ActivableHandlers = new();

        private static readonly List<IUpdate> s_Update = new();

        public static void RegisterUpdateHandler(Entity entity, IUpdate handler)
        {
            if (!s_UpdateHandlers.TryGetValue(entity, out HashSet<IUpdate> handlers))
            {
                handlers = new HashSet<IUpdate>();
                s_UpdateHandlers.Add(entity, handlers);
            }
            handlers.Add(handler);
        }

        public static void UnregisterUpdateHandler(Entity entity, IUpdate handler)
        {
            if (s_UpdateHandlers.TryGetValue(entity, out HashSet<IUpdate> handlers))
            {
                handlers.Remove(handler);
            }
        }

        public static void RegisterActivableHandler(Entity entity, IActivable handler)
        {
            if (!s_ActivableHandlers.TryGetValue(entity, out HashSet<IActivable> handlers))
            {
                handlers = new HashSet<IActivable>();
                s_ActivableHandlers.Add(entity, handlers);
            }
            handlers.Add(handler);
        }

        public static void UnregisterActivableHandler(Entity entity, IActivable handler)
        {
            if (s_ActivableHandlers.TryGetValue(entity, out HashSet<IActivable> handlers))
            {
                handlers.Remove(handler);
            }
        }

        public static void SetEntityActive(Entity entity, bool isActive)
        {
            if (s_ActivableHandlers.TryGetValue(entity, out HashSet<IActivable> handlers))
            {
                foreach (IActivable handler in handlers)
                {
                    handler.OnSetActive(isActive);
                }
            }
        }
    }
}