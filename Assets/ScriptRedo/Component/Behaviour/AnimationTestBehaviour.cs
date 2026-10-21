using UnityEngine;
using UnityEngine.InputSystem;

namespace ScriptRedo
{
    public sealed class AnimationTestBehaviour : IBehaviour
    {
        private AnimationPlayableGraph m_Graph;

        //当一些重要的组件找不到（被置空）时，整个行为不能正常运行就用这个做标记，这样以避免每次访问时都得判空。所以至少得两个或者以上的组件需要判空时这个方式才能有用，且只能是简单逻辑
        //有些逻辑不能直接中断就不能使用这个方式
        private bool m_CanDoBehaviour;

        public void OnAddToEntity(Entity entity)
        {
            m_Graph = entity.GetComponent<AnimationPlayableGraph>();
            entity.RegisterComponentReferenceChangeListener(typeof(AnimationPlayableGraph), ListenAnimationPlayableGraph);
        }

        public void OnRemoveFromEntity(Entity entity)
        {
            m_Graph = null;
            entity.UnregisterComponentReferenceChangeListener(typeof(AnimationPlayableGraph), ListenAnimationPlayableGraph);
        }

        public bool IsRunning()
        {
            return false;
        }

        public void Start()
        {
            m_Graph.Play();
        }

        public void Stop()
        {
            m_Graph.Stop();
        }

        public void Reset()
        {
            m_Graph.Reset();
        }

        private void Update()
        {
            if (m_CanDoBehaviour)
            {
                float velocity = Random.value;
                velocity = Mathf.Clamp01(velocity);
                //混合idle->walk->run，权重值通过velocity计算，比如0->0.5
                if (velocity <= 0.5f)
                {
                    //混合idle->walk，这个weight是walk最终的权重，idle的权重为1-weight
                    float weight = velocity / 0.5f;
                    m_Graph.Blend();
                }
                else
                {
                    //混合walk->run，这个weight是run最终的权重，walk的权重为1-weight
                    float weight = (velocity - 0.5f) / 0.5f;
                    m_Graph.Blend();
                }

                if (Keyboard.current.aKey.wasPressedThisFrame)
                {
                    //混合到移动动画
                    m_Graph.Blend();
                }
                if (Keyboard.current.spaceKey.wasPressedThisFrame)
                {
                    //混合到跳跃动画
                    m_Graph.Blend();
                }
            }
        }

        private void ListenAnimationPlayableGraph(IComponent component)
        {
            if (component != null)
            {
                m_Graph = (AnimationPlayableGraph)component;
            }
            else
            {
                m_Graph = null;
                m_CanDoBehaviour = false;
            }
        }
    }
}