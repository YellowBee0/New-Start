using System.Collections.Generic;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace ScriptRedo
{
    public sealed class AnimationMixerInstance
    {
        private readonly AnimationPlayableGraph m_Graph;

        private readonly AnimationMixerPlayable m_MixerPlayable;

        private readonly List<AnimationClipInstance> m_AnimationClipInstances = new();

        public AnimationMixerInstance(PlayableGraph graph)
        {
            m_MixerPlayable = AnimationMixerPlayable.Create(graph);
        }

        public AnimationClipInstance GetAnimationClipInstance(int portIndex)
        {
            return m_AnimationClipInstances[portIndex];
        }

        public void AddAnimationClipInstance(AnimationClipInstance animationClipInstance)
        {
        }

        public void RemoveAnimationClipInstance(AnimationClipInstance animationClipInstance)
        {
        }

        public void SetInputWeight(int portIndex, float weight)
        {
            m_MixerPlayable.SetInputWeight(portIndex, weight);
        }
    }
}