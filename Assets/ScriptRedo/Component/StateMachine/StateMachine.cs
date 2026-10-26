namespace ScriptRedo
{
    public sealed class StateMachine : IComponent
    {
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
                    m_CurrentState.Exit();
                }
                m_TransitionState.AddParticipantState(m_CurrentState, false);
                m_TransitionState.AddParticipantState(nextState, true);
                m_CurrentState = m_TransitionState;
                m_CurrentState.Enter();
            }
        }

        public void OnAddToEntity(Entity entity)
        {
        }

        public void OnRemoveFromEntity(Entity entity)
        {
        }
    }
}