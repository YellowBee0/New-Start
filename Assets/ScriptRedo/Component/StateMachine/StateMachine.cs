namespace ScriptRedo
{
    public sealed class StateMachine : IComponent
    {
        private IState[] m_States;

        private readonly TransitionState m_TransitionState;

        private IState m_CurrentState;

        public IState GetCurrentState()
        {
            return m_CurrentState;
        }

        public void ChangeState(string stateName)
        {
            //enter: walk:walk weight:1,test weight:1;run:walk weight:1 speed:2;
            //exit: walk:walk weight:0,test weight:0;run:walk weight:0 speed:1;
        }

        public void ChangeState(IState newState)
        {
            if (m_CurrentState != newState)
            {
                //这里使用oldState保存当前状态的原因：过渡状态需要在Exit的时候获取当前状态机准确的状态是否为过渡状态想要切换的目标状态，
                //如果不是过渡状态需要把目标状态设置的权重全部重置，所以需要先更新当前状态是哪一个，然后执行exit和enter。对于过渡切换可以不用注意这点
                IState oldState = m_CurrentState;
                m_CurrentState = newState;
                if (oldState != null)
                {
                    oldState.OnExit();
                }
                if (newState != null)
                {
                    newState.OnEnter();
                }
            }
        }

        public void TransitionTo(IState nextState)
        {
            if (m_CurrentState != nextState)
            {
                if (m_CurrentState != m_TransitionState)
                {
                    //能够过渡的条件：当前状态是继承TransitionParticipantState的或者目标状态是继承TransitionParticipantState的
                    bool canTranslate = nextState is TransitionParticipantState;
                    if (m_CurrentState != null)
                    {
                        if (m_CurrentState is TransitionParticipantState participantState)
                        {
                            participantState.IsControlled(true);
                            canTranslate = true;
                        }
                        if (canTranslate)
                        {
                            //添加参与过渡的状态
                            m_TransitionState.Add(m_CurrentState);
                            m_CurrentState.OnExit();
                        }
                    }
                    if (canTranslate)
                    {
                        m_CurrentState = m_TransitionState;
                        m_CurrentState.OnEnter(); 
                    }
                    else
                    {
                        ChangeState(nextState);
                        return;
                    }
                }
                //设置目标状态
                m_TransitionState.SetTargetState(nextState);
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