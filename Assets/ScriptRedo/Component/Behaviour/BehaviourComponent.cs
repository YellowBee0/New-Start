namespace ScriptRedo
{
    public sealed class BehaviourComponent : IBehaviourComponent<IBehaviour>
    {
        private Entity m_Entity;

        private BehaviourComponentData m_Data;

        private IBehaviour m_Behaviour;

        public void InitializeData(BehaviourComponentData data)
        {
            m_Data = data;
        }

        public void OnAddToEntity(Entity entity)
        {
            m_Entity = entity;
            IRuntimeData<IBehaviour> behaviourData = m_Data.GetBehaviourData();
            if (behaviourData != null)
            {
                m_Behaviour = behaviourData.CreateRuntimeInstance();
                m_Behaviour.OnAddToEntity(entity);
                if (m_Data.StartOnAddToEntityAndStopOnRemoveFromEntity)
                {
                    m_Behaviour.Start();
                }
            }
        }

        public void OnRemoveFromEntity(Entity entity)
        {
            //需要处理如果是在切换了行为状态下，是否需要回到以前的行为，然后再移除？还是直接移除当前行为，与之切换的行为就不能再恢复了？
            if (m_Behaviour != null)
            {
                if (m_Data.StartOnAddToEntityAndStopOnRemoveFromEntity)
                {
                    m_Behaviour.Stop();
                }
                m_Behaviour.OnRemoveFromEntity(entity);
            }
            m_Entity = null;
        }

        public void ChangeBehaviour(IBehaviour behaviourToChange)
        {
            bool isRunning = false;
            if (m_Behaviour != null)
            {
                isRunning = m_Behaviour.IsRunning();
                if (isRunning)
                {
                    m_Behaviour.Stop();
                }
                m_Behaviour.Reset();
                m_Behaviour.OnRemoveFromEntity(m_Entity);
            }
            m_Behaviour = behaviourToChange;
            if (m_Behaviour != null)
            {
                m_Behaviour.OnAddToEntity(m_Entity);
                if (isRunning)
                {
                    m_Behaviour.Start();
                }
            }
        }

        //需要注册Entity启用和禁用，分别在这两个里面决定是否Start还是Stop
    }
}