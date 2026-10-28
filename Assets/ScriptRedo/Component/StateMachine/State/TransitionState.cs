using System.Collections.Generic;

namespace ScriptRedo
{
    public sealed class TransitionState : IState
    {
        private readonly List<TransitionParticipantState> m_TransitionParticipantStates = new();

        private TransitionParticipantState m_TargetState;

        private IState m_NormalTargetState;

        private StateMachine m_StateMachine;

        private bool m_IsTranslate;

        public void OnEnter()
        {
            m_IsTranslate = true;
        }

        public void OnExit()
        {
            for (int i = 0; i < m_TransitionParticipantStates.Count; i++)
            {
                TransitionParticipantState participantState = m_TransitionParticipantStates[i];
                m_TargetState.AfterStateExit();
                //填入具体key
                participantState.ResetWeight(new[] { "" });
                participantState.IsControlled(false);
            }
            if (m_StateMachine.GetCurrentState() != m_TargetState && m_StateMachine.GetCurrentState() != m_NormalTargetState)
            {
                //如果状态机最终切换的状态不是过渡目标状态，过渡目标状态的权重直接设置为0。这个会发生在过渡状态过渡时，通过直接切换切换到其他状态（非过渡目标状态）
                m_TargetState.AfterStateExit();
                m_TargetState.ResetWeight(null);
            }
            m_TransitionParticipantStates.Clear();
            m_TargetState = null;
            m_NormalTargetState = null;
            m_IsTranslate = false;
        }

        public void OnUpdate(float deltaTime)
        {
            for (int i = 0; i < m_TransitionParticipantStates.Count; i++)
            {
                //填入具体key
                m_TransitionParticipantStates[i].OnTransitionUpdate(new[] { "" }, deltaTime);
            }
            if (m_TargetState != null)
            {
                //填入具体key
                m_TargetState.OnTransitionUpdate(new[] { "" }, 1 - deltaTime);
            }
            //过渡时间到了调用StateMachine的ChangeState(targetState)
        }

        public IState GetTargetState()
        {
            return m_TargetState ?? m_NormalTargetState;
        }

        //添加参与过渡的状态
        public void Add(IState state)
        {
        }

        //设置目标状态
        public void SetTargetState(IState state)
        {
            if (m_IsTranslate && state is TransitionParticipantState participantState)
            {
                participantState.BeforeStateEnter();
            }
        }
    }
}