using System;
using System.Collections.Generic;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace ScriptRedo
{
    //一个循环动画过渡如何写的示例
    public sealed class TransitionableExampleState : ITransitionableState
    {
        private bool m_IsRunning;

        private bool m_IsTransitioning;

        private AnimationMixerPlayable m_Mixer;

        private int m_IdleIndex;

        public bool IsRunning()
        {
            return m_IsRunning;
        }

        public void Start()
        {
            m_IsRunning = true;
            m_Mixer.SetInputWeight(m_IdleIndex, 1);
            //判断是否在播放中
            m_Mixer.GetInput(m_IdleIndex).Play();
        }

        public void Stop()
        {
            m_IsRunning = false;
            if (!m_IsTransitioning)
            {
                m_Mixer.GetInput(m_IdleIndex).Pause();
            }
        }

        public void Reset()
        {
            m_Mixer.SetInputWeight(m_IdleIndex, 0);
        }

        public float GetWeight()
        {
            throw new NotImplementedException();
        }

        public IReadOnlyList<string> GetCrossKeys()
        {
            throw new NotImplementedException();
        }

        public void SetIsTransitioning(bool isTransitioning)
        {
            throw new NotImplementedException();
        }

        public void OnCrossInStart()
        {
            m_Mixer.GetInput(m_IdleIndex).Play();
        }

        public void OnCrossOutEnd(IReadOnlyList<string> keys)
        {
            if (keys == null)
            {
                m_Mixer.GetInput(m_IdleIndex).Pause();
            }
        }

        public void OnUpdateCrossIn(float deltaWeight)
        {
            throw new NotImplementedException();
        }

        public void OnUpdateCrossOut(IReadOnlyList<string> crossKeys, float deltaWeight)
        {
            throw new NotImplementedException();
        }
    }
}