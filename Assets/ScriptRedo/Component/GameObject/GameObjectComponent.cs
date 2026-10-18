using UnityEngine;

namespace ScriptRedo
{
    public sealed class GameObjectComponent : IComponent
    {
        //通过预制体实例化的对象
        private GameObject m_GameObjectInstance;

        public int m_int;
        
        public void OnAddToEntity(Entity entity)
        {
            throw new System.NotImplementedException();
        }

        public void OnRemoveFromEntity(Entity entity)
        {
            throw new System.NotImplementedException();
        }
    }
}