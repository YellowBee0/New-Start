namespace ScriptRedo
{
    public sealed class BuffBehaviourComponent : IBehaviourComponent<IBuffBehaviour>
    {
        private BuffManagerComponent m_BuffManager;

        private Entity m_Entity;

        private Entity m_Caster;

        private IBuffBehaviour m_BuffBehaviour;

        private BuffBehaviourComponentData m_BuffBehaviourData;

        public void OnAddToEntity(Entity entity)
        {
            m_Entity = entity;
            //m_BuffBehaviour.OnAddToEntity(entity);
        }

        public void OnRemoveFromEntity(Entity entity)
        {
            m_Entity = null;
            //m_BuffBehaviour.OnRemoveFromEntity(entity);
        }

        public void ChangeBehaviour(IBuffBehaviour behaviourToChange)
        {
            bool isRunning = false;
            if (m_BuffBehaviour != null)
            {
                isRunning = m_BuffBehaviour.IsRunning();
                if (isRunning)
                {
                    m_BuffBehaviour.Stop();
                }
                m_BuffBehaviour.Reset();
                m_BuffBehaviour.OnRemoveFromBuffManager(m_BuffManager);
                //m_BuffBehaviour.OnRemoveFromEntity(m_Entity);
            }
            m_BuffBehaviour = behaviourToChange;
            if (m_BuffBehaviour != null)
            {
                //m_BuffBehaviour.OnAddToEntity(m_Entity);
                m_BuffBehaviour.OnAddToBuffManager(m_BuffManager);
                if (isRunning)
                {
                    m_BuffBehaviour.Start();
                }
            }
        }

        public void InitializeData(BuffBehaviourComponentData data)
        {
            m_BuffBehaviourData = data;
            m_BuffBehaviour = data.BuffBehaviourData.CreateRuntimeInstance();
        }

        public void OnAddToBuffManager(BuffManagerComponent buffManager)
        {
            m_BuffManager = buffManager;
            m_BuffBehaviour.OnAddToBuffManager(buffManager);
        }

        public void OnRemoveFromBuffManager(BuffManagerComponent buffManager)
        {
            m_BuffManager = null;
            m_BuffBehaviour.OnRemoveFromBuffManager(buffManager);
        }
    }
}