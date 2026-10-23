using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

namespace DistributedAnimationStates
{
    public enum AnimationWeightBlendMode
    {
        // Both raw input levels interpolate independently. Their product can be nonlinear.
        InputWeights,
        // Interpolate L and L*W, then recover W. This avoids an extra fade on an empty layer.
        EffectiveContribution
    }

    public readonly struct AnimationClipKey : IEquatable<AnimationClipKey>
    {
        public readonly AnimationLayerDefinition Layer;
        public readonly AnimationClip Clip;

        public AnimationClipKey(AnimationLayerDefinition layer, AnimationClip clip)
        {
            Layer = layer;
            Clip = clip;
        }

        public bool Equals(AnimationClipKey other) { return Layer == other.Layer && Clip == other.Clip; }
        public override bool Equals(object obj) { return obj is AnimationClipKey && Equals((AnimationClipKey)obj); }
        public override int GetHashCode()
        {
            unchecked { return (ReferenceEquals(Layer, null) ? 0 : Layer.GetHashCode()) * 397 ^
                (ReferenceEquals(Clip, null) ? 0 : Clip.GetHashCode()); }
        }
    }

    /// <summary>Absent keys mean zero. Weights are raw mixer inputs, never silently normalized.</summary>
    public sealed class AnimationWeightSnapshot
    {
        private readonly Dictionary<AnimationLayerDefinition, float> layers =
            new Dictionary<AnimationLayerDefinition, float>();
        private readonly Dictionary<AnimationClipKey, float> clips = new Dictionary<AnimationClipKey, float>();

        public IReadOnlyDictionary<AnimationLayerDefinition, float> LayerWeights { get; }
        public IReadOnlyDictionary<AnimationClipKey, float> ClipWeights { get; }

        public AnimationWeightSnapshot()
        {
            LayerWeights = new ReadOnlyDictionary<AnimationLayerDefinition, float>(layers);
            ClipWeights = new ReadOnlyDictionary<AnimationClipKey, float>(clips);
        }

        public float GetLayerWeight(AnimationLayerDefinition layer)
        {
            float value;
            return layer != null && layers.TryGetValue(layer, out value) ? value : 0;
        }

        public float GetClipWeight(AnimationClipKey key)
        {
            float value;
            return clips.TryGetValue(key, out value) ? value : 0;
        }

        public void SetLayerWeight(AnimationLayerDefinition layer, float weight)
        {
            if (layer == null) throw new ArgumentNullException(nameof(layer));
            ValidateWeight(weight);
            layers[layer] = weight;
        }

        public void SetClipWeight(AnimationClipKey key, float weight)
        {
            if (key.Layer == null || key.Clip == null) throw new ArgumentException("Invalid clip key.");
            ValidateWeight(weight);
            clips[key] = weight;
        }

        public AnimationWeightSnapshot Clone()
        {
            var result = new AnimationWeightSnapshot();
            foreach (var item in layers) result.layers.Add(item.Key, item.Value);
            foreach (var item in clips) result.clips.Add(item.Key, item.Value);
            return result;
        }

        public static AnimationWeightSnapshot Interpolate(AnimationWeightSnapshot from,
            AnimationWeightSnapshot to, float t, AnimationWeightBlendMode blendMode)
        {
            if (from == null || to == null) throw new ArgumentNullException();
            if (float.IsNaN(t) || float.IsInfinity(t)) throw new ArgumentOutOfRangeException(nameof(t));
            if (blendMode != AnimationWeightBlendMode.InputWeights &&
                blendMode != AnimationWeightBlendMode.EffectiveContribution)
                throw new ArgumentOutOfRangeException(nameof(blendMode));
            if (t <= 0) return from.Clone();
            if (t >= 1) return to.Clone();
            var result = new AnimationWeightSnapshot();
            var layerKeys = new HashSet<AnimationLayerDefinition>(from.layers.Keys);
            layerKeys.UnionWith(to.layers.Keys);
            // Also cover a hidden clip whose caller intentionally omitted a layer entry.
            foreach (var key in from.clips.Keys) layerKeys.Add(key.Layer);
            foreach (var key in to.clips.Keys) layerKeys.Add(key.Layer);
            foreach (var key in layerKeys)
                result.SetLayerWeight(key, Mathf.Lerp(from.GetLayerWeight(key), to.GetLayerWeight(key), t));
            var clipKeys = new HashSet<AnimationClipKey>(from.clips.Keys);
            clipKeys.UnionWith(to.clips.Keys);
            foreach (var key in clipKeys)
            {
                float a = from.GetClipWeight(key), b = to.GetClipWeight(key);
                float weight;
                if (blendMode == AnimationWeightBlendMode.EffectiveContribution)
                {
                    float layer = result.GetLayerWeight(key.Layer);
                    weight = layer > 0 ? Mathf.Lerp(from.GetLayerWeight(key.Layer) * a,
                        to.GetLayerWeight(key.Layer) * b, t) / layer : Mathf.Lerp(a, b, t);
                }
                else weight = Mathf.Lerp(a, b, t);
                // Roundoff after division must not produce an invalid unit weight.
                result.SetClipWeight(key, Mathf.Clamp01(weight));
            }
            return result;
        }

        private static void ValidateWeight(float value)
        {
            if (float.IsNaN(value) || float.IsInfinity(value) || value < 0 || value > 1)
                throw new ArgumentOutOfRangeException(nameof(value), "Weight must be finite and in [0, 1].");
        }
    }
}
