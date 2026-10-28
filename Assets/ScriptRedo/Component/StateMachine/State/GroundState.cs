using System;
using System.Collections.Generic;
using UnityEngine.Animations;
using UnityEngine.Playables;
using Random = UnityEngine.Random;

namespace ScriptRedo
{
    public sealed class GroundState : IState
    {
        private AnimationMixerPlayable m_AnimationMixer;

        private readonly int m_IdleIndex = 0;

        private readonly int m_WalkIndex = 1;

        private readonly int m_RunIndex = 2;

        public void OnEnter()
        {
            //读取现在的速度
            for (int i = 0; i < 3; i++)
            {
                m_AnimationMixer.GetInput(i).Play();
            }
            float velocity = Random.Range(0, 1);
            SetWeight(velocity);
        }

        public void OnExit()
        {
            for (int i = 0; i < 3; i++)
            {
                m_AnimationMixer.SetInputWeight(i, 0);
                m_AnimationMixer.GetInput(i).Pause();
            }
        }

        public void OnUpdate(float deltaTime)
        {
            float velocity = Random.Range(0, 1);
            SetWeight(velocity);
        }

        private void SetWeight(float velocity)
        {
            if (velocity < 0)
            {
                velocity = 0;
            }
            float weight;
            if (velocity <= 1)
            {
                weight = velocity;
                m_AnimationMixer.SetInputWeight(m_IdleIndex, weight);
                m_AnimationMixer.SetInputWeight(m_WalkIndex, 1 - weight);
                m_AnimationMixer.SetInputWeight(m_RunIndex, 0);
            }
            else
            {
                weight = Math.Clamp(velocity - 1, 0, 1);
                m_AnimationMixer.SetInputWeight(m_IdleIndex, 0);
                m_AnimationMixer.SetInputWeight(m_WalkIndex, 1 - weight);
                m_AnimationMixer.SetInputWeight(m_RunIndex, weight);
            }
        }
    }

    public sealed class TranslateGroundState : TransitionParticipantState
    {
        private AnimationMixerPlayable m_AnimationMixer;

        private readonly int m_IdleIndex = 0;

        private readonly int m_WalkIndex = 1;

        private readonly int m_RunIndex = 2;

        private void SetWeight(float velocity)
        {
            if (velocity < 0)
            {
                velocity = 0;
            }
            float weight;
            if (velocity <= 1)
            {
                weight = velocity;
                m_AnimationMixer.SetInputWeight(m_IdleIndex, weight);
                m_AnimationMixer.SetInputWeight(m_WalkIndex, 1 - weight);
                m_AnimationMixer.SetInputWeight(m_RunIndex, 0);
            }
            else
            {
                weight = Math.Clamp(velocity - 1, 0, 1);
                m_AnimationMixer.SetInputWeight(m_IdleIndex, 0);
                m_AnimationMixer.SetInputWeight(m_WalkIndex, 1 - weight);
                m_AnimationMixer.SetInputWeight(m_RunIndex, weight);
            }
        }

        public override void OnUpdate(float deltaTime)
        {
            float velocity = Random.Range(0, 1);
            SetWeight(velocity);
        }

        public override IReadOnlyList<string> GetTransitionKeys()
        {
            return new[] { "Idle", "Walk", "Run" };
        }

        public override void OnTransitionUpdate(IReadOnlyList<string> transitionKeys, float deltaWeight)
        {
            if (deltaWeight > 0)
            {
                //表示过渡切换的目标是当前状态
                //同样可以判断velocity，计算velocity在那个区间，算出0-1的权重比例，然后乘以当前状态的权重，分别设置给每个动画片段
                //当前为1，目标通过velocity算出为0.8，(1-0.8)*deltaWeight/(1-weight)(需要判断1-Weight是否大于0，避免除0或者除负数)算出一帧改变值，开始时改变量很小，越到后面改变量越大
            }
            for (int i = 0; i < transitionKeys.Count; i++)
            {
                switch (transitionKeys[i])
                {
                    case "Idle":
                        float currentIdleWeight = m_AnimationMixer.GetInputWeight(m_IdleIndex);
                        m_AnimationMixer.SetInputWeight(m_IdleIndex, currentIdleWeight + currentIdleWeight * deltaWeight);
                        break;
                    case "Walk":
                        float currentWalkWeight = m_AnimationMixer.GetInputWeight(m_WalkIndex);
                        m_AnimationMixer.SetInputWeight(m_WalkIndex, currentWalkWeight + currentWalkWeight * deltaWeight);
                        break;
                    case "Run":
                        float currentRunWeight = m_AnimationMixer.GetInputWeight(m_RunIndex);
                        m_AnimationMixer.SetInputWeight(m_RunIndex, currentRunWeight + currentRunWeight * deltaWeight);
                        break;
                }
            }
        }

        protected override void OnNormalEnter()
        {
            float velocity = Random.Range(0, 1);
            SetWeight(velocity);
        }

        protected override void OnNormalExit()
        {
            //在退出的时候权重归0是必要的，如果不设置过渡状态会保留前一个动画的权重，如果新动画带有权重开始播放就会导致权重所用动画权重之和超过1
            for (int i = 0; i < 3; i++)
            {
                m_AnimationMixer.SetInputWeight(i, 0);
            }
        }

        protected override void OnResetWeight(IReadOnlyList<string> transitionKeys)
        {
            for (int i = 0; i < transitionKeys.Count; i++)
            {
                switch (transitionKeys[i])
                {
                    case "Idle":
                        m_AnimationMixer.SetInputWeight(m_IdleIndex, 0);
                        break;
                    case "Walk":
                        m_AnimationMixer.SetInputWeight(m_WalkIndex, 0);
                        break;
                    case "Run":
                        m_AnimationMixer.SetInputWeight(m_RunIndex, 0);
                        break;
                }
            }
        }

        public override void BeforeStateEnter()
        {
            for (int i = 0; i < 3; i++)
            {
                m_AnimationMixer.GetInput(i).Play();
            }
        }

        public override void AfterStateExit()
        {
            for (int i = 0; i < 3; i++)
            {
                m_AnimationMixer.GetInput(i).Pause();
            }
        }
    }
}