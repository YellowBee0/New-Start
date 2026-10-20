using System;
using UnityEngine;

namespace ScriptRedo
{
    public sealed class GameObjectComponent : IComponent
    {
        //通过预制体实例化的对象
        private GameObject m_GameObjectInstance;

        public void OnAddToEntity(Entity entity)
        {
            throw new NotImplementedException();
        }

        public void OnRemoveFromEntity(Entity entity)
        {
            throw new NotImplementedException();
        }

        public T GetComponent<T>(string path) where T : Component
        {
            if (m_GameObjectInstance != null)
            {
                Transform targetGameObject = m_GameObjectInstance.transform.Find(path);
                if (targetGameObject != null)
                {
                    return targetGameObject.GetComponent<T>();
                }
            }
            return null;
        }
    }
}