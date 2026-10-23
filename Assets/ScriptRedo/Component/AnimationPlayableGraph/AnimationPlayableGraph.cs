using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace ScriptRedo
{
    public sealed class AnimationPlayableGraph : IComponent
    {
        private readonly unsafe struct CreateInvokerWrapper
        {
            private readonly delegate*<string, IAnimationEventInvoker> m_Action;

            public CreateInvokerWrapper(delegate*<string, IAnimationEventInvoker> action)
            {
                m_Action = action;
            }

            public IAnimationEventInvoker Invoke(string eventName)
            {
                return m_Action(eventName);
            }
        }

        private AnimationPlayableGraphData m_Data;

        private Animator m_Animator;

        private PlayableGraph m_Graph;

        private AnimationPlayableOutput m_Output;

        private AnimationLayerMixerPlayable m_LayerMixerPlayable;

        private (AnimationMixerPlayable, List<AnimationClipInstance>)[] m_PlayablesInLayer;

        //全局的动画事件提供者
        private static readonly Dictionary<string, CreateInvokerWrapper> s_StaticProviders = new();

        //只属于当前这个实体的动画事件提供者
        private readonly List<IAnimationEventProvider> m_LocalProviders = new();

        public void OnAddToEntity(Entity entity)
        {
            GameObjectComponent gameObjectComponent = entity.GetComponent<GameObjectComponent>();
            if (gameObjectComponent != null)
            {
                m_Animator = gameObjectComponent.GetComponent<Animator>(m_Data.AnimatorPath);
                if (m_Animator != null)
                {
                    CreateGraph(m_Animator);
                }
            }
            entity.RegisterComponentReferenceChangeListener(typeof(GameObjectComponent), ListenGameObjectComponent);
        }

        public void OnRemoveFromEntity(Entity entity)
        {
            entity.UnregisterComponentReferenceChangeListener(typeof(GameObjectComponent), ListenGameObjectComponent);
            m_Animator = null;
            m_Graph.Destroy();
        }

        public void InitializeData(AnimationPlayableGraphData data)
        {
            m_Data = data;
        }

        private void ListenGameObjectComponent(IComponent component)
        {
            Animator animator;
            if (component != null)
            {
                GameObjectComponent gameObjectComponent = (GameObjectComponent)component;
                animator = gameObjectComponent.GetComponent<Animator>(m_Data.AnimatorPath);
            }
            else
            {
                animator = null;
            }
            ChangeAnimator(animator);
        }

        private void ChangeAnimator(Animator animator)
        {
            if (m_Animator != animator)
            {
                if (m_Animator != null)
                {
                    if (animator == null)
                    {
                        m_Graph.Stop();
                    }
                    m_Output.SetTarget(animator);
                }
                else
                {
                    CreateGraph(animator);
                }
                m_Animator = animator;
            }
        }

        private void CreateGraph(Animator animator)
        {
            m_Graph = PlayableGraph.Create("Test");
            m_Output = AnimationPlayableOutput.Create(m_Graph, string.Empty, animator);
            m_LayerMixerPlayable = AnimationLayerMixerPlayable.Create(m_Graph);
            //根据Data创建Layer
            m_Output.SetSourcePlayable(m_LayerMixerPlayable);
        }

        public void Play()
        {
            m_Graph.Play();
        }

        public void Stop()
        {
            m_Graph.Stop();
        }

        public void Reset()
        {
            //重置所有权重不为0的动画Time为0
        }

        //多个动画混合涉及的参数：权重、播放速度和动画长度用这三个参数目的是为了让参与混合的动画在相同的deltaTime中推进相同的动画百分比进度
        //这样会存在speed很快、动画长度很短的动画混合speed很慢、动画长度很长的动画时一个变慢一个变快
        public void Blend()
        {
        }
    }

    public sealed class Animation
    {
        private AnimationLayerMixerPlayable m_LayerMixerPlayable;

        private AnimationMixerPlayable m_MixerPlayable;
        
        private AnimationClipPlayable m_ClipPlayable;
        
        private List<AnimationEventData> m_AnimationEvents = new();
    }
}