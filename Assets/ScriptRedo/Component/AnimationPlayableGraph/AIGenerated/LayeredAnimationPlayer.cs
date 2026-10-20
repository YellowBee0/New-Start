using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace LayeredPlayables
{
    /// <summary>
    /// Owns one Animator. Uses manual sampling for in-place animations.
    /// Set up Layers before Initialize; layer 0 is a full-weight override base.
    /// </summary>
    [DisallowMultipleComponent, RequireComponent(typeof(Animator))]
    public sealed class LayeredAnimationPlayer : MonoBehaviour
    {
        public AnimationLayerSettings[] Layers =
        {
            new AnimationLayerSettings("Base"),
            new AnimationLayerSettings("Upper Body")
        };
        public bool AutoUpdate = true;
        public bool UseUnscaledTime;
        [Tooltip("Events below this effective clip * layer weight are suppressed.")]
        [Range(0, 1)] public float EventWeightThreshold = 0.01f;

        public event Action<AnimationEventContext> EventReceived;
        public bool IsPaused { get; private set; }
        public bool IsInitialized { get { return graph.IsValid(); } }

        private sealed class ClipState
        {
            public PlayableAnimationClip Asset;
            public AnimationClipPlayable Playable;
            public TimelineMarker[] Markers;
            public int Port;
            public double Time;
            public double Duration;
            public float Speed;
            public float Weight, FromWeight, TargetWeight;
            public bool Loop, EmitEvents, IncludeStart = true, Alive = true;
            public int Generation;
        }

        private sealed class LayerState
        {
            public AnimationMixerPlayable Mixer;
            public readonly List<ClipState> Clips = new List<ClipState>();
            public readonly Stack<int> FreePorts = new Stack<int>();
            public bool Paused;
            public float FadeTime, FadeDuration;
            public float Weight, FromWeight, TargetWeight, WeightTime, WeightDuration;
        }

        private readonly struct PendingEvent
        {
            public readonly ClipState State;
            public readonly int Generation;
            public readonly AnimationEventContext Context;
            public PendingEvent(ClipState state, AnimationEventContext context)
            {
                State = state;
                Generation = state.Generation;
                Context = context;
            }
        }

        private PlayableGraph graph;
        private AnimationLayerMixerPlayable layerMixer;
        private LayerState[] states;
        private Animator animator;
        private bool oldFireEvents, oldRootMotion;
        private AnimatorCullingMode oldCulling;
        private bool ticking;
        private bool poseDirty;
        private int dispatchEpoch;
        private readonly List<TimelineOccurrence> occurrences = new List<TimelineOccurrence>();
        private readonly List<PendingEvent> pendingEvents = new List<PendingEvent>();

        public void Initialize()
        {
            if (IsInitialized) return;
            if (Layers == null || Layers.Length == 0)
                throw new InvalidOperationException("Configure at least one animation layer.");
            for (int i = 0; i < Layers.Length; i++)
            {
                if (Layers[i] == null) throw new ArgumentException("Layer settings cannot be null.");
                UnitWeight(Layers[i].Weight, "Layers.Weight");
            }
            if (Layers[0].Mask != null || Layers[0].Additive || Layers[0].Weight != 1)
                throw new ArgumentException("Layer 0 must have weight 1, no mask and Additive=false.");

            animator = GetComponent<Animator>();
            if (animator.runtimeAnimatorController != null)
                throw new InvalidOperationException("Set this Animator's Controller to None first.");

            oldFireEvents = animator.fireEvents;
            oldRootMotion = animator.applyRootMotion;
            oldCulling = animator.cullingMode;
            animator.fireEvents = false;
            animator.applyRootMotion = false;
            animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
            try
            {
                graph = PlayableGraph.Create(name + ".LayeredAnimation");
                graph.SetTimeUpdateMode(DirectorUpdateMode.Manual);
                layerMixer = AnimationLayerMixerPlayable.Create(graph, Layers.Length, false);
                states = new LayerState[Layers.Length];
                for (int i = 0; i < states.Length; i++)
                {
                    var config = Layers[i];
                    var layer = new LayerState
                    {
                        Mixer = AnimationMixerPlayable.Create(graph, 0),
                        Weight = config.Weight,
                        FromWeight = config.Weight,
                        TargetWeight = config.Weight
                    };
                    states[i] = layer;
                    graph.Connect(layer.Mixer, 0, layerMixer, i);
                    layerMixer.SetInputWeight(i, 0);
                    layerMixer.SetLayerAdditive((uint)i, config.Additive);
                    if (config.Mask != null)
                        layerMixer.SetLayerMaskFromAvatarMask((uint)i, config.Mask);
                }
                var output = AnimationPlayableOutput.Create(graph, "Animation", animator);
                output.SetSourcePlayable(layerMixer);
                graph.Play();
            }
            catch
            {
                ReleaseGraph();
                throw;
            }
        }

        private void Update()
        {
            if (AutoUpdate) Tick(UseUnscaledTime ? UnityEngine.Time.unscaledDeltaTime : UnityEngine.Time.deltaTime);
        }

        /// <summary>Set AutoUpdate=false when driving this clock yourself.</summary>
        public void Tick(float deltaTime)
        {
            NonNegative(deltaTime, nameof(deltaTime));
            if (ticking) throw new InvalidOperationException("Tick cannot be called recursively.");
            Initialize();
            if (!isActiveAndEnabled || IsPaused) return;

            ticking = true;
            pendingEvents.Clear();
            try
            {
                for (int i = 0; i < states.Length; i++) AdvanceLayer(i, deltaTime);
                poseDirty = false;
                graph.Evaluate(0);
                // Dispatch only after graph evaluation and structural changes are complete.
                int epoch = dispatchEpoch;
                for (int i = 0; i < pendingEvents.Count; i++)
                {
                    if (!IsInitialized || IsPaused || epoch != dispatchEpoch || !isActiveAndEnabled) break;
                    var item = pendingEvents[i];
                    if (!item.State.Alive || item.State.Generation != item.Generation ||
                        states[item.Context.LayerIndex].Paused) continue;
                    try { EventReceived?.Invoke(item.Context); }
                    catch (Exception exception) { Debug.LogException(exception, this); }
                }
                // Callback commands must be visible this frame, even after Play + Stop.
                if (poseDirty && IsInitialized && isActiveAndEnabled)
                {
                    poseDirty = false;
                    graph.Evaluate(0);
                }
            }
            finally
            {
                pendingEvents.Clear();
                ticking = false;
            }
        }

        public void Play(int layer, PlayableAnimationClip animation, float fadeDuration = 0.2f,
            float speed = 1, bool emitEvents = true)
        {
            Blend(layer, new[] { new AnimationBlendInput(animation, 1, speed, emitEvents) }, fadeDuration);
        }

        /// <summary>
        /// Weights are normalized. Existing clips keep their clocks; Replay resets them.
        /// Passing the same blend repeatedly is unnecessary: call on target changes.
        /// </summary>
        public void Blend(int layer, AnimationBlendInput[] inputs, float fadeDuration = 0.2f)
        {
            var target = GetLayer(layer);
            NonNegative(fadeDuration, nameof(fadeDuration));
            if (inputs == null || inputs.Length == 0)
                throw new ArgumentException("Blend requires at least one positive-weight input.");
            double total = 0;
            var unique = new HashSet<PlayableAnimationClip>();
            // Validate the whole request before changing the current transition.
            var markerSnapshots = new TimelineMarker[inputs.Length][];
            for (int i = 0; i < inputs.Length; i++)
            {
                var input = inputs[i];
                ValidateAsset(input.Animation);
                NonNegative(input.Weight, "input.Weight");
                NonNegative(input.Speed, "input.Speed");
                if (!unique.Add(input.Animation)) throw new ArgumentException("Duplicate animation in a blend.");
                total += input.Weight;
                markerSnapshots[i] = input.Animation.SnapshotEvents();
            }
            if (total <= 0) throw new ArgumentException("The sum of blend weights must be positive.");

            bool wasEmpty = target.Clips.Count == 0;
            foreach (var clip in target.Clips)
            {
                clip.FromWeight = clip.Weight;
                clip.TargetWeight = 0;
            }
            for (int i = 0; i < inputs.Length; i++)
            {
                var input = inputs[i];
                if (input.Weight <= 0) continue;
                var clip = Find(target, input.Animation);
                if (clip == null)
                {
                    int port = target.FreePorts.Count > 0 ? target.FreePorts.Pop() : target.Mixer.GetInputCount();
                    if (port == target.Mixer.GetInputCount()) target.Mixer.SetInputCount(port + 1);
                    var playable = AnimationClipPlayable.Create(graph, input.Animation.Clip);
                    playable.SetSpeed(0); // Our clock is the only source of time.
                    playable.SetApplyFootIK(input.Animation.ApplyFootIK);
                    playable.SetApplyPlayableIK(false);
                    playable.SetDuration(double.MaxValue);
                    playable.SetTime(0);
                    graph.Connect(playable, 0, target.Mixer, port);
                    clip = new ClipState
                    {
                        Asset = input.Animation, Playable = playable, Port = port,
                        Duration = input.Animation.Clip.length, Loop = input.Animation.Clip.isLooping,
                        Markers = markerSnapshots[i]
                    };
                    target.Clips.Add(clip);
                }
                clip.Speed = input.Speed;
                clip.EmitEvents = input.EmitEvents;
                clip.TargetWeight = (float)(input.Weight / total);
            }
            target.FadeTime = 0;
            target.FadeDuration = wasEmpty ? 0 : fadeDuration;
            ApplyClipWeights(target, target.FadeDuration == 0 ? 1 : 0);
            RemoveFadedClips(target);
            RefreshLayerOutput(layer);
            SampleIfIdle();
        }

        public void SetLayerWeight(int layer, float weight, float fadeDuration = 0)
        {
            var target = GetLayer(layer);
            UnitWeight(weight, nameof(weight));
            NonNegative(fadeDuration, nameof(fadeDuration));
            if (layer == 0 && weight != 1) throw new ArgumentException("Base layer weight must stay 1.");
            target.FromWeight = target.Weight;
            target.TargetWeight = weight;
            target.WeightTime = 0;
            target.WeightDuration = fadeDuration;
            if (fadeDuration == 0) target.Weight = weight;
            RefreshLayerOutput(layer);
            SampleIfIdle();
        }

        /// <summary>Freeze time, transitions and events, keeping the current pose.</summary>
        public void Stop() { IsPaused = true; dispatchEpoch++; }
        public void Pause() { Stop(); }
        public void Resume() { IsPaused = false; }

        /// <summary>Restart current target clips, finish any crossfade, and resume all layers.</summary>
        public void Replay()
        {
            Initialize();
            dispatchEpoch++;
            for (int i = 0; i < states.Length; i++) ResetLayer(i);
            IsPaused = false;
            SampleIfIdle();
        }

        public void StopLayer(int layer) { GetLayer(layer).Paused = true; }
        public void ResumeLayer(int layer) { GetLayer(layer).Paused = false; }
        // Does not unpause a globally stopped player; call Resume() as well in that case.
        public void ReplayLayer(int layer) { GetLayer(layer); ResetLayer(layer); SampleIfIdle(); }

        /// <summary>Remove all clips from a layer, exposing the layers below it.</summary>
        public void ClearLayer(int layer)
        {
            var target = GetLayer(layer);
            for (int i = target.Clips.Count - 1; i >= 0; i--) RemoveClip(target, i);
            RefreshLayerOutput(layer);
            SampleIfIdle();
        }

        public double GetTime(int layer, PlayableAnimationClip animation)
        {
            var clip = Find(GetLayer(layer), animation);
            if (clip == null) throw new ArgumentException("Animation is not active on this layer.");
            return clip.Time;
        }
        public float GetClipWeight(int layer, PlayableAnimationClip animation)
        {
            var clip = Find(GetLayer(layer), animation);
            return clip == null ? 0 : clip.Weight;
        }
        public float GetLayerWeight(int layer) { return GetLayer(layer).Weight; }
        public int GetActiveClipCount(int layer) { return GetLayer(layer).Clips.Count; }

        private void AdvanceLayer(int index, float dt)
        {
            var layer = states[index];
            if (layer.Paused) return;
            layer.FadeTime += dt;
            layer.WeightTime += dt;
            float blend = layer.FadeDuration <= 0 ? 1 : Mathf.Clamp01(layer.FadeTime / layer.FadeDuration);
            float weightBlend = layer.WeightDuration <= 0 ? 1 : Mathf.Clamp01(layer.WeightTime / layer.WeightDuration);
            layer.Weight = Mathf.Lerp(layer.FromWeight, layer.TargetWeight, weightBlend);
            ApplyClipWeights(layer, blend);

            foreach (var clip in layer.Clips)
            {
                double previous = clip.Time;
                double next = previous + (double)dt * clip.Speed;
                if (!clip.Loop) next = Math.Min(next, clip.Duration);
                occurrences.Clear();
                // Suppressed events are consumed rather than emitted later as a burst.
                bool audible = clip.EmitEvents && clip.Weight > 0 && layer.Weight > 0 &&
                    clip.Weight * layer.Weight >= EventWeightThreshold;
                if (audible && (next > previous || (clip.IncludeStart && dt > 0 && clip.Speed > 0)))
                {
                    AnimationEventTimeline.Collect(clip.Markers, previous, next, clip.Duration,
                        clip.Loop, clip.IncludeStart, occurrences);
                    foreach (var occurrence in occurrences)
                        pendingEvents.Add(new PendingEvent(clip, new AnimationEventContext(index, clip.Asset, occurrence)));
                }
                if (dt > 0 && clip.Speed > 0) clip.IncludeStart = false;
                clip.Time = next;
                clip.Playable.SetDone(false);
                clip.Playable.SetTime(clip.Loop ? next % clip.Duration : next);
            }
            RemoveFadedClips(layer);
            RefreshLayerOutput(index);
        }

        private static void ApplyClipWeights(LayerState layer, float blend)
        {
            foreach (var clip in layer.Clips)
            {
                clip.Weight = Mathf.Lerp(clip.FromWeight, clip.TargetWeight, blend);
                layer.Mixer.SetInputWeight(clip.Port, clip.Weight);
            }
        }

        private void RemoveFadedClips(LayerState layer)
        {
            for (int i = layer.Clips.Count - 1; i >= 0; i--)
                if (layer.Clips[i].Weight == 0 && layer.Clips[i].TargetWeight == 0) RemoveClip(layer, i);
        }

        private void RemoveClip(LayerState layer, int index)
        {
            var clip = layer.Clips[index];
            clip.Alive = false;
            layer.Mixer.SetInputWeight(clip.Port, 0);
            graph.Disconnect(layer.Mixer, clip.Port);
            graph.DestroyPlayable(clip.Playable);
            layer.FreePorts.Push(clip.Port);
            layer.Clips.RemoveAt(index);
        }

        private void ResetLayer(int index)
        {
            var layer = states[index];
            layer.Paused = false;
            layer.FadeTime = layer.FadeDuration;
            ApplyClipWeights(layer, 1);
            RemoveFadedClips(layer);
            foreach (var clip in layer.Clips)
            {
                clip.Time = 0;
                clip.IncludeStart = true;
                clip.Generation++;
                clip.Playable.SetDone(false);
                clip.Playable.SetTime(0);
            }
            // Keep the current layer weight; cancel its unfinished weight transition.
            layer.FromWeight = layer.TargetWeight = layer.Weight;
            layer.WeightTime = layer.WeightDuration = 0;
            RefreshLayerOutput(index);
        }

        private LayerState GetLayer(int index)
        {
            Initialize();
            if (index < 0 || index >= states.Length) throw new ArgumentOutOfRangeException(nameof(index));
            return states[index];
        }

        private static ClipState Find(LayerState layer, PlayableAnimationClip asset)
        {
            foreach (var clip in layer.Clips) if (clip.Asset == asset) return clip;
            return null;
        }

        private void RefreshLayerOutput(int index)
        {
            var layer = states[index];
            layerMixer.SetInputWeight(index, layer.Clips.Count == 0 ? 0 : layer.Weight);
        }

        private void SampleIfIdle()
        {
            if (!IsInitialized || !isActiveAndEnabled) return;
            if (ticking) poseDirty = true;
            else graph.Evaluate(0);
        }

        private static void ValidateAsset(PlayableAnimationClip asset)
        {
            if (asset == null || asset.Clip == null) throw new ArgumentException("Animation/Clip is missing.");
            if (asset.Clip.legacy) throw new ArgumentException("Legacy AnimationClip is not supported.");
            if (!(asset.Clip.length > 0) || float.IsInfinity(asset.Clip.length))
                throw new ArgumentException("AnimationClip length must be finite and positive.");
        }

        private static void NonNegative(float value, string name)
        {
            if (float.IsNaN(value) || float.IsInfinity(value) || value < 0)
                throw new ArgumentOutOfRangeException(name, "Must be finite and non-negative.");
        }

        private static void UnitWeight(float value, string name)
        {
            NonNegative(value, name);
            if (value > 1) throw new ArgumentOutOfRangeException(name, "Must be in [0, 1].");
        }

        // Preserve clocks when the component is disabled; resume automatically on enable.
        private void OnEnable() { SampleIfIdle(); }
        private void OnDisable() { dispatchEpoch++; }
        private void OnDestroy() { ReleaseGraph(); }

        private void ReleaseGraph()
        {
            dispatchEpoch++;
            poseDirty = false;
            if (states != null)
                foreach (var layer in states)
                    if (layer != null) foreach (var clip in layer.Clips) clip.Alive = false;
            if (graph.IsValid()) graph.Destroy();
            states = null;
            if (animator != null)
            {
                animator.fireEvents = oldFireEvents;
                animator.applyRootMotion = oldRootMotion;
                animator.cullingMode = oldCulling;
            }
        }
    }
}
