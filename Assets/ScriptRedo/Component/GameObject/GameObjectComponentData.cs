using System;
using UnityEngine;

namespace ScriptRedo
{
    [Serializable]
    public sealed class GameObjectComponentData : IRuntimeData<GameObjectComponent>
    {
        public GameObject Prefab;

        public GameObjectComponent CreateRuntimeInstance()
        {
            GameObjectComponent component = new();
            //预制体有一个对象池管理
            //GameObjectComponent组件有一个对象池管理
            //这两个是组合关系，每次在这里创建新的GameObjectComponent时都需要重新赋值GameObjectComponent的m_GameObjectInstance，不能使用以前的m_GameObjectInstance
            return component;
        }
    }
}