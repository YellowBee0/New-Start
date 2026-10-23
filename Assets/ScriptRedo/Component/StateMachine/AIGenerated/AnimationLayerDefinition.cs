using UnityEngine;

namespace DistributedAnimationStates
{
    /// <summary>Asset identity is the layer key. Array order in the driver determines stacking order.</summary>
    [CreateAssetMenu(menuName = "Animation States/Layer Definition")]
    public sealed class AnimationLayerDefinition : ScriptableObject
    {
        public string DisplayName;
        public AvatarMask Mask;
        public bool Additive;
    }
}
