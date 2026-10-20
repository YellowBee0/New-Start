using System;
using System.Collections.Generic;
using UnityEngine;

namespace LayeredPlayables
{
    [Serializable]
    public sealed class AnimationEventMarker
    {
        [Range(0, 1)] public float NormalizedTime;
        public string Id;
        public string Payload;
    }

    [CreateAssetMenu(menuName = "Animation/Playable Animation Clip")]
    public sealed class PlayableAnimationClip : ScriptableObject
    {
        public AnimationClip Clip;
        public bool ApplyFootIK;
        public List<AnimationEventMarker> Events = new List<AnimationEventMarker>();

        internal TimelineMarker[] SnapshotEvents()
        {
            if (Events == null) return new TimelineMarker[0];
            var result = new TimelineMarker[Events.Count];
            for (int i = 0; i < result.Length; i++)
            {
                var marker = Events[i];
                if (marker == null || float.IsNaN(marker.NormalizedTime) ||
                    marker.NormalizedTime < 0 || marker.NormalizedTime > 1)
                    throw new ArgumentException(name + ": event time must be in [0, 1].");
                result[i] = new TimelineMarker(marker.NormalizedTime, marker.Id, marker.Payload);
            }
            return result;
        }
    }

    [Serializable]
    public sealed class AnimationLayerSettings
    {
        public string Name;
        [Range(0, 1)] public float Weight = 1;
        public AvatarMask Mask;
        public bool Additive;

        public AnimationLayerSettings(string name, float weight = 1,
            AvatarMask mask = null, bool additive = false)
        {
            Name = name;
            Weight = weight;
            Mask = mask;
            Additive = additive;
        }
    }

    public readonly struct AnimationBlendInput
    {
        public readonly PlayableAnimationClip Animation;
        public readonly float Weight;
        public readonly float Speed;
        public readonly bool EmitEvents;

        public AnimationBlendInput(PlayableAnimationClip animation, float weight = 1,
            float speed = 1, bool emitEvents = true)
        {
            Animation = animation;
            Weight = weight;
            Speed = speed;
            EmitEvents = emitEvents;
        }
    }

    public readonly struct AnimationEventContext
    {
        public readonly int LayerIndex;
        public readonly PlayableAnimationClip Animation;
        public readonly string EventId;
        public readonly string Payload;
        public readonly long Cycle;
        // Absolute clip clock in seconds, not wall-clock time.
        public readonly double Time;

        internal AnimationEventContext(int layerIndex, PlayableAnimationClip animation,
            TimelineOccurrence occurrence)
        {
            LayerIndex = layerIndex;
            Animation = animation;
            EventId = occurrence.Marker.Id;
            Payload = occurrence.Marker.Payload;
            Cycle = occurrence.Cycle;
            Time = occurrence.AbsoluteTime;
        }
    }
}
