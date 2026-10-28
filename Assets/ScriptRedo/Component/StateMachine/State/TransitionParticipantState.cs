using System.Collections.Generic;

namespace ScriptRedo
{
    public abstract class TransitionParticipantState : IState
    {
        private float m_Weight;

        private bool m_TranslatorControlled;

        public float GetWeight()
        {
            return m_Weight;
        }

        public void IsControlled(bool isControlled)
        {
            m_TranslatorControlled = isControlled;
        }

        public void OnEnter()
        {
            m_Weight = 1;
            OnNormalEnter();
            if (!m_TranslatorControlled)
            {
                BeforeStateEnter();
            }
            else
            {
                m_TranslatorControlled = false;
            }
        }

        public void OnExit()
        {
            //先执行不设置权重的Exit
            OnNormalExit();
            //是否需要保持现在的权重（在过渡开始时需要状态保持当前的权重）
            if (!m_TranslatorControlled)
            {
                AfterStateExit();
                ResetWeight(null);
            }
        }

        public abstract void OnUpdate(float deltaTime);

        public void ResetWeight(IReadOnlyList<string> transitionKeys)
        {
            m_Weight = 0;
            OnResetWeight(transitionKeys);
        }

        public abstract IReadOnlyList<string> GetTransitionKeys();

        public abstract void OnTransitionUpdate(IReadOnlyList<string> transitionKeys, float deltaWeight);

        protected abstract void OnNormalEnter();

        protected abstract void OnNormalExit();

        protected abstract void OnResetWeight(IReadOnlyList<string> transitionKeys);

        public virtual void BeforeStateEnter()
        {
        }

        public virtual void AfterStateExit()
        {
        }
    }
}