using System.Collections.Generic;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace ScriptRedo
{
    public sealed class AnimationClipInstance
    {
        private AnimationClipPlayable m_AnimationClipPlayable;

        private List<AnimationEventData> m_AnimationEvents = new();

        public AnimationClipPlayable GetAnimationClipPlayable()
        {
            return m_AnimationClipPlayable;
        }

        public void Play()
        {
            m_AnimationClipPlayable.Play();
            //开始执行动画事件
        }

        public void Stop()
        {
            m_AnimationClipPlayable.Pause();
            //暂停动画事件执行
        }

        public void Reset()
        {
            SetTime(0);
        }

        public void GetTime()
        {
            m_AnimationClipPlayable.GetTime();
        }

        public void SetTime(float time)
        {
            m_AnimationClipPlayable.SetTime(time);
            //动画事件执行时间也定格到time
        }

        public void Destroy()
        {
            m_AnimationClipPlayable.Destroy();
            //释放动画事件
        }
    }
}