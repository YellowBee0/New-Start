using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace DistributedAnimationStates
{
    /// <summary>
    /// A weight-only Playables manager. No autonomous Update and no built-in fades.
    /// One clock: call Tick once after the state machine has applied this frame's weights.
    /// An existing graph can instead implement IAnimationWeightDriver directly.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class PlayableAnimationWeightDriver : MonoBehaviour, IAnimationWeightDriver, IDisposable
    {
        public Animator TargetAnimator;
        [Tooltip("Stacking order, bottom to top. State definitions reference these layer assets.")]
        public AnimationLayerDefinition[] Layers = new AnimationLayerDefinition[0];
        public bool ApplyFootIK;
        public bool ApplyPlayableIK;

        private sealed class ClipInput
        {
            public AnimationClip Clip;
            public AnimationClipPlayable Playable;
            public int Port;
        }

        private sealed class LayerInput
        {
            public AnimationLayerDefinition Definition;
            public AnimationMixerPlayable Mixer;
            public int Port;
            public readonly Dictionary<AnimationClip, ClipInput> Clips = new Dictionary<AnimationClip, ClipInput>();
        }

        private PlayableGraph graph;
        private AnimationPlayableOutput output;
        private AnimationLayerMixerPlayable layerMixer;
        private readonly Dictionary<AnimationLayerDefinition, LayerInput> inputs =
            new Dictionary<AnimationLayerDefinition, LayerInput>();
        private bool ticking;
        private bool poseDirty;
        private bool disposePending;
        private bool destroyed;

        public bool IsInitialized { get { return graph.IsValid(); } }
        public bool IsPaused { get; private set; }

        public void Initialize()
        {
            if (destroyed) throw new ObjectDisposedException(nameof(PlayableAnimationWeightDriver));
            if (IsInitialized) return;
            if (Layers == null || Layers.Length == 0)
                throw new ArgumentException("Configure at least one layer on the weight driver.");
            var unique = new HashSet<AnimationLayerDefinition>();
            foreach (var layer in Layers)
                if (layer == null || !unique.Add(layer))
                    throw new ArgumentException("Driver layers must be non-null and unique.");
            if (TargetAnimator == null) TargetAnimator = GetComponent<Animator>();
            ValidateAnimator(TargetAnimator);
            try
            {
                graph = PlayableGraph.Create(name + ".AnimationStates");
                graph.SetTimeUpdateMode(DirectorUpdateMode.Manual);
                // Reserve an unanimated base at port 0 so every user layer has a real blend weight.
                // Disable the single-layer optimization; all user layers start at port 1.
                layerMixer = AnimationLayerMixerPlayable.Create(graph, Layers.Length + 1, false);
                var neutral = AnimationMixerPlayable.Create(graph, 0);
                graph.Connect(neutral, 0, layerMixer, 0);
                layerMixer.SetInputWeight(0, 1);
                for (int i = 0; i < Layers.Length; i++)
                {
                    var definition = Layers[i];
                    var layer = new LayerInput
                    {
                        Definition = definition,
                        Port = i + 1,
                        Mixer = AnimationMixerPlayable.Create(graph, 0)
                    };
                    inputs.Add(definition, layer);
                    graph.Connect(layer.Mixer, 0, layerMixer, layer.Port);
                    layerMixer.SetInputWeight(layer.Port, 0);
                    layerMixer.SetLayerAdditive((uint)layer.Port, definition.Additive);
                    if (definition.Mask != null)
                        layerMixer.SetLayerMaskFromAvatarMask((uint)layer.Port, definition.Mask);
                }
                output = AnimationPlayableOutput.Create(graph, "Animation States", TargetAnimator);
                output.SetSourcePlayable(layerMixer);
                graph.Play();
            }
            catch
            {
                Dispose();
                throw;
            }
        }

        public void Prepare(AnimationWeightSnapshot target)
        {
            EnsureNotEvaluating();
            if (target == null) throw new ArgumentNullException(nameof(target));
            Initialize();
            ValidateFrame(target);
            // Validate the entire frame before adding any zero-weight inputs.
            foreach (var item in target.ClipWeights)
            {
                var layer = inputs[item.Key.Layer];
                if (layer.Clips.ContainsKey(item.Key.Clip)) continue;
                var playable = AnimationClipPlayable.Create(graph, item.Key.Clip);
                try
                {
                    playable.SetApplyFootIK(ApplyFootIK);
                    playable.SetApplyPlayableIK(ApplyPlayableIK);
                    playable.SetDuration(double.MaxValue);
                    playable.SetTime(0);
                    playable.SetSpeed(0);
                    int port = layer.Mixer.GetInputCount();
                    layer.Mixer.SetInputCount(port + 1);
                    graph.Connect(playable, 0, layer.Mixer, port);
                    layer.Mixer.SetInputWeight(port, 0);
                    layer.Clips.Add(item.Key.Clip, new ClipInput
                    {
                        Clip = item.Key.Clip, Playable = playable, Port = port
                    });
                }
                catch
                {
                    if (playable.IsValid()) graph.DestroyPlayable(playable);
                    throw;
                }
            }
        }

        public AnimationWeightSnapshot Capture()
        {
            Initialize();
            var result = new AnimationWeightSnapshot();
            foreach (var layer in inputs.Values)
            {
                result.SetLayerWeight(layer.Definition, layerMixer.GetInputWeight(layer.Port));
                foreach (var clip in layer.Clips.Values)
                    result.SetClipWeight(new AnimationClipKey(layer.Definition, clip.Clip),
                        layer.Mixer.GetInputWeight(clip.Port));
            }
            return result;
        }

        public void Apply(AnimationWeightSnapshot weights)
        {
            // Prepare validates the full request before any currently visible weight is changed.
            Prepare(weights);
            foreach (var layer in inputs.Values)
            {
                layerMixer.SetInputWeight(layer.Port, weights.GetLayerWeight(layer.Definition));
                foreach (var clip in layer.Clips.Values)
                    layer.Mixer.SetInputWeight(clip.Port,
                        weights.GetClipWeight(new AnimationClipKey(layer.Definition, clip.Clip)));
            }
            poseDirty = true;
            // No Evaluate here: changes form a single batch, sampled once by Tick.
        }

        public void Tick(float deltaTime)
        {
            if (float.IsNaN(deltaTime) || float.IsInfinity(deltaTime) || deltaTime < 0)
                throw new ArgumentOutOfRangeException(nameof(deltaTime));
            if (ticking) throw new InvalidOperationException("Graph Tick cannot be called recursively.");
            Initialize();
            if (IsPaused) { SampleIfDirty(); return; }
            foreach (var layer in inputs.Values)
            {
                bool visible = layerMixer.GetInputWeight(layer.Port) > 0;
                foreach (var clip in layer.Clips.Values)
                    clip.Playable.SetSpeed(visible && layer.Mixer.GetInputWeight(clip.Port) > 0 ? 1 : 0);
            }
            Evaluate(deltaTime);
        }

        /// <summary>Sample changed weights/times without advancing clocks, even while paused.</summary>
        public void SampleIfDirty()
        {
            EnsureNotEvaluating();
            if (IsInitialized && poseDirty) Evaluate(0);
        }

        public double GetTime(AnimationLayerDefinition layer, AnimationClip clip)
        {
            Initialize();
            LayerInput layerInput;
            ClipInput clipInput;
            return inputs.TryGetValue(layer, out layerInput) && layerInput.Clips.TryGetValue(clip, out clipInput)
                ? clipInput.Playable.GetTime() : 0;
        }

        public void SetAnimator(Animator animator)
        {
            EnsureNotEvaluating();
            ValidateAnimator(animator);
            TargetAnimator = animator;
            if (IsInitialized) { output.SetTarget(animator); poseDirty = true; }
        }

        public void Pause() { IsPaused = true; }
        public void Resume() { IsPaused = false; }

        /// <summary>Restart currently weighted clips. Does not change states, weights or transition progress.</summary>
        public void Replay()
        {
            EnsureNotEvaluating();
            Initialize();
            foreach (var layer in inputs.Values)
                if (layerMixer.GetInputWeight(layer.Port) > 0)
                    foreach (var clip in layer.Clips.Values)
                        if (layer.Mixer.GetInputWeight(clip.Port) > 0)
                        {
                            clip.Playable.SetTime(0);
                            clip.Playable.SetDone(false);
                        }
            poseDirty = true;
        }

        /// <summary>Restart every input listed in a state/snapshot, including hidden incoming clips.</summary>
        public void Replay(AnimationWeightSnapshot selection)
        {
            Prepare(selection);
            foreach (var item in selection.ClipWeights)
            {
                var clip = inputs[item.Key.Layer].Clips[item.Key.Clip];
                clip.Playable.SetTime(0);
                clip.Playable.SetDone(false);
            }
            poseDirty = true;
        }

        public void Dispose()
        {
            if (ticking) { disposePending = true; return; }
            DisposeGraph();
        }

        private void DisposeGraph()
        {
            if (graph.IsValid()) graph.Destroy();
            graph = default(PlayableGraph);
            inputs.Clear();
            disposePending = false;
            poseDirty = false;
        }

        private void Evaluate(float deltaTime)
        {
            ticking = true;
            poseDirty = false;
            try { graph.Evaluate(deltaTime); }
            finally
            {
                ticking = false;
                if (disposePending) DisposeGraph();
            }
        }

        private void EnsureNotEvaluating()
        {
            if (ticking)
                throw new InvalidOperationException("Queue graph changes until after its animation callbacks finish.");
        }

        private void ValidateFrame(AnimationWeightSnapshot frame)
        {
            foreach (var item in frame.LayerWeights)
                if (item.Key == null || !inputs.ContainsKey(item.Key))
                    throw new ArgumentException("The state references a layer not configured on this driver.");
            foreach (var item in frame.ClipWeights)
                if (item.Key.Layer == null || !inputs.ContainsKey(item.Key.Layer) ||
                    item.Key.Clip == null || item.Key.Clip.legacy)
                    throw new ArgumentException("The frame contains an unknown layer or invalid/Legacy clip.");
        }

        private static void ValidateAnimator(Animator animator)
        {
            if (animator != null && animator.runtimeAnimatorController != null)
                throw new ArgumentException("Use an Animator whose Controller is None.");
        }

        private void OnDestroy() { destroyed = true; Dispose(); }
    }
}
