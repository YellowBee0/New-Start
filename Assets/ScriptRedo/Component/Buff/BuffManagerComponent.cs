using System.Collections.Generic;

namespace ScriptRedo
{
    public sealed class BuffManagerComponent : IComponent
    {
        private List<BuffBehaviourComponent> m_Buffs;

        public void OnAddToEntity(Entity entity)
        {
            for (int i = 0; i < m_Buffs.Count; i++)
            {
                m_Buffs[i].OnAddToBuffManager(this);
            }
        }

        public void OnRemoveFromEntity(Entity entity)
        {
            for (int i = 0; i < m_Buffs.Count; i++)
            {
                m_Buffs[i].OnRemoveFromBuffManager(this);
            }
        }

        public void AddBuff(Entity caster, EntityAsset entityAsset)
        {
            BuffBehaviourComponentData buffBehaviourComponentData = entityAsset.GetComponentData<BuffBehaviourComponentData>();
            if (buffBehaviourComponentData != null)
            {
                //比较是否为重复添加
                Entity buffEntity = entityAsset.CreateRuntimeInstance();
                BuffBehaviourComponent buffBehaviourComponent = buffEntity.GetComponent<BuffBehaviourComponent>();
                m_Buffs.Add(buffBehaviourComponent);
            }
        }
    }
}