namespace ScriptRedo
{
    public sealed class StateMachine : IComponent
    {
        private AnimationPlayableGraph m_Graph;

        /*private State m_CurrentState;

        private State[] m_States;*/

        private IState[] m_States;

        private readonly TransitionState m_TransitionState;

        private IState m_CurrentState;

        public void ChangeState(string stateName)
        {
            //enter: walk:walk weight:1,test weight:1;run:walk weight:1 speed:2;
            //exit: walk:walk weight:0,test weight:0;run:walk weight:0 speed:1;
        }

        public void TransitionTo(IState nextState)
        {
            if (m_CurrentState == m_TransitionState)
            {
                m_TransitionState.AddParticipantState(m_TransitionState.GetTargetState(), false);
                m_TransitionState.AddParticipantState(nextState, true);
            }
            else
            {
                if (m_CurrentState != null)
                {
                    m_CurrentState.OnExit();
                }
                m_TransitionState.AddParticipantState(m_CurrentState, false);
                m_TransitionState.AddParticipantState(nextState, true);
                m_CurrentState = m_TransitionState;
                m_CurrentState.OnEnter();
            }
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