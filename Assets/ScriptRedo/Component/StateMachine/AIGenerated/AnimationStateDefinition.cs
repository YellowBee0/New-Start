using System;
using System.Collections.Generic;
using UnityEngine;

namespace DistributedAnimationStates
{
    [Serializable]
    public sealed class AnimationStateClip
    {
        public AnimationClip Clip;
        [Range(0, 1)] public float Weight = 1;
    }

    [Serializable]
    public sealed class AnimationStateLayer
    {
        public AnimationLayerDefinition Layer;
        [Range(0, 1)] public float Weight = 1;
        public AnimationStateClip[] Clips = new AnimationStateClip[0];
    }

    /// <summary>Shared authoring data only; clocks and transition progress never live in this asset.</summary>
    [CreateAssetMenu(menuName = "Animation States/State Definition")]
    public sealed class AnimationStateDefinition : ScriptableObject
    {
        public string StateName;
        public AnimationStateLayer[] Layers = new AnimationStateLayer[0];

        public AnimationWeightSnapshot CreateSnapshot()
        {
            if (string.IsNullOrWhiteSpace(StateName))
                throw new ArgumentException(name + ": StateName is required.");
            if (Layers == null) throw new ArgumentException(StateName + ": Layers is null.");
            var result = new AnimationWeightSnapshot();
            var uniqueLayers = new HashSet<AnimationLayerDefinition>();
            for (int i = 0; i < Layers.Length; i++)
            {
                var layer = Layers[i];
                if (layer == null || layer.Layer == null)
                    throw new ArgumentException(StateName + ": layer reference is missing.");
                if (!uniqueLayers.Add(layer.Layer))
                    throw new ArgumentException(StateName + ": duplicate layer " + layer.Layer.name);
                result.SetLayerWeight(layer.Layer, layer.Weight);
                if (layer.Clips == null) throw new ArgumentException(StateName + ": Clips is null.");
                var uniqueClips = new HashSet<AnimationClip>();
                bool hasPositiveClip = false;
                foreach (var clip in layer.Clips)
                {
                    if (clip == null || clip.Clip == null || clip.Clip.legacy)
                        throw new ArgumentException(StateName + ": a non-Legacy AnimationClip is required.");
                    if (!uniqueClips.Add(clip.Clip))
                        throw new ArgumentException(StateName + ": duplicate clip in one layer.");
                    result.SetClipWeight(new AnimationClipKey(layer.Layer, clip.Clip), clip.Weight);
                    hasPositiveClip |= clip.Weight > 0;
                }
                if (layer.Weight > 0 && !hasPositiveClip)
                    throw new ArgumentException(StateName + ": a visible layer requires a positive clip weight.");
            }
            return result;
        }
    }
}
