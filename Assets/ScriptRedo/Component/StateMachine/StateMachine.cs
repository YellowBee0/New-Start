namespace ScriptRedo
{
    public sealed class StateMachine : IComponent
    {
        private AnimationPlayableGraph m_Graph;

        private State m_CurrentState;

        private State[] m_States;

        public void ChangeState(string stateName)
        {
            
        }
        
        public void OnAddToEntity(Entity entity)
        {
            m_Graph = entity.GetComponent<AnimationPlayableGraph>();
            entity.RegisterComponentReferenceChangeListener(typeof(AnimationPlayableGraph), ListenAnimationPlayableGraph);
        }

        public void OnRemoveFromEntity(Entity entity)
        {
            m_Graph = entity.GetComponent<AnimationPlayableGraph>();
            entity.RegisterComponentReferenceChangeListener(typeof(AnimationPlayableGraph), ListenAnimationPlayableGraph);
        }

        private void ListenAnimationPlayableGraph(IComponent component)
        {
            if (component != null)
            {
                m_Graph = (AnimationPlayableGraph)component;
            }
            else
            {
                m_Graph = null;
            }
        }
    }
}