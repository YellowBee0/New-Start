using System.Collections.Generic;

namespace ScriptRedo
{
    public sealed class TransitionState : IState
    {
        private readonly List<TransitionParticipantState> m_TransitionParticipantStates = new();

        private TransitionParticipantState m_TargetState;

        private IState m_NormalTargetState;

        public void Enter()
        {
            for (int i = 0; i < m_TransitionParticipantStates.Count; i++)
            {
                m_TransitionParticipantStates[i].Exit();
            }
        }

        public void Exit()
        {
            for (int i = 0; i < m_TransitionParticipantStates.Count; i++)
            {
                TransitionParticipantState participantState = m_TransitionParticipantStates[i];
                participantState.TransitionExit();
                participantState.SetKeepWeightOnExit(false);
            }
            m_TransitionParticipantStates.Clear();
            m_TargetState = null;
            m_NormalTargetState = null;
        }

        public void Update(float deltaTime)
        {
            for (int i = 0; i < m_TransitionParticipantStates.Count; i++)
            {
                m_TransitionParticipantStates[i].TransitionUpdate(, deltaTime);
            }
            if (m_TargetState != null)
            {
                m_TargetState.TransitionUpdate(, 1 - deltaTime);
            }
            //过渡时间到了调用StateMachine的ChangeState(targetState)
        }

        public IState GetTargetState()
        {
            return m_TargetState ?? m_NormalTargetState;
        }
        
        public void AddParticipantState(IState state, bool isTarget)
        {
            if (state is TransitionParticipantState participantState)
            {
                if (isTarget)
                {
                    m_TargetState = participantState;
                }
                else
                {
                    participantState.SetKeepWeightOnExit(true);
                    m_TransitionParticipantStates.Add(participantState);
                }
            }
            else
            {
                if (isTarget)
                {
                    m_NormalTargetState = state;
                }
            }
        }
    }
}