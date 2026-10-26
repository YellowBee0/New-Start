using System.Collections.Generic;

namespace ScriptRedo
{
    public abstract class TransitionParticipantState : IState
    {
        private float m_Weight;

        private bool m_KeepWeightOnExit;

        public float GetWeight()
        {
            return m_Weight;
        }

        public void SetKeepWeightOnExit(bool isKeepWeightOnExit)
        {
            m_KeepWeightOnExit = isKeepWeightOnExit;
        }

        public void OnEnter()
        {
            m_Weight = 1;
            Enter();
        }

        public void OnExit()
        {
            //先执行不设置权重的Exit
            NormalExit();
            //是否需要保持现在的权重（在过渡开始时需要状态保持当前的权重）
            if (!m_KeepWeightOnExit)
            {
                TransitionExit(null);
            }
        }

        public void TransitionExit(IReadOnlyList<string> transitionKeys)
        {
            m_Weight = 0;
            OnTransitionExit(transitionKeys);
        }

        public abstract void OnUpdate(float deltaTime);

        public abstract IReadOnlyList<string> GetTransitionKeys();

        protected abstract void Enter();

        protected abstract void NormalExit();
        
        protected abstract void OnTransitionExit(IReadOnlyList<string> transitionKeys);

        public abstract void OnTransitionUpdate(IReadOnlyList<string> transitionKeys, float deltaWeight);
    }

    //有两个Layer，一个上半身Upper，一个下半身Lower
    //Upper的攻击动画，需要响应动画事件，Lower的跑步动画，需要响应RootMotion和动画事件

    //这个类对应的数据：状态名Idle、Run、Attack或者RunAttack用于索引；需要调用播放的动画（动画数据包括：在哪一个Layer下；在Mixer的哪个端口下（动画索引）；播放权重）；

    /*public sealed class IdleState : BaseState
    {
        private const float ENTER_IDLE_WEIGHT = 1;

        private const float EXIT_IDLE_WEIGHT = 0;

        private AnimationMixerPlayable m_Layer1Mixer;

        public override void OnUpdate(float deltaTime)
        {
            throw new NotImplementedException();
        }

        public override IReadOnlyList<string> GetCrossKeys()
        {
            return new[] { "Idle" };
        }

        protected override void OnEnter()
        {
            m_Layer1Mixer.SetInputWeight(0, ENTER_IDLE_WEIGHT);
        }

        protected override void OnExit()
        {
            m_Layer1Mixer.SetInputWeight(0, EXIT_IDLE_WEIGHT);
        }

        protected override void OnUpdateCross(IReadOnlyList<string> keys, float deltaWeight)
        {
            for (int i = 0; i < keys.Count; i++)
            {
                switch (keys[i])
                {
                    case "Idle":
                        float currentWeight = m_Layer1Mixer.GetInputWeight(0);
                        m_Layer1Mixer.SetInputWeight(i, currentWeight + deltaWeight);
                        break;
                }
            }
        }
    }

    public sealed class StateCrosser
    {
        private readonly List<BaseState> m_ExitStates = new();

        private readonly List<string> m_CrossKeys = new();

        private BaseState m_EnterState;

        public void Update()
        {
            int value = 10;
            for (int i = 0; i < value; i++)
            {
                //0-10为需要退出状态要更新的key
            }
            for (int i = 10; i < m_CrossKeys.Count; i++)
            {
                //10-数组长度为需要进入状态要更新的key
            }
        }
    }*/
}