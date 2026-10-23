using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace DistributedAnimationStates
{
    public enum AnimationStateTransition
    {
        Immediate,
        CrossFade
    }

    /// <summary>
    /// Registers definitions supplied by independent gameplay components and applies their
    /// weights through one driver. Definitions are identified by asset reference; names are
    /// display labels. This class neither owns the driver nor resets animation clip clocks.
    /// Call Tick once per frame from the owning component, on Unity's main thread.
    /// </summary>
    public sealed class AnimationStateMachine
    {
        private readonly IAnimationWeightDriver driver;
        private readonly Dictionary<AnimationStateDefinition, Registration> registrations =
            new Dictionary<AnimationStateDefinition, Registration>(StateReferenceComparer.Instance);

        private AnimationWeightSnapshot transitionFrom;
        private AnimationWeightSnapshot transitionTo;
        private AnimationWeightBlendMode transitionBlendMode;
        private double transitionElapsed;
        private double transitionDuration;
        private long generation;
        private bool ticking;
        private bool driverBusy;

        /// <summary>The target state, including while a crossfade is still running.</summary>
        public AnimationStateDefinition CurrentState { get; private set; }
        public bool IsTransitioning { get; private set; }
        /// <summary>Zero after Clear, zero at fade start, and one after entry completes.</summary>
        public float TransitionProgress { get; private set; }
        public bool IsPaused { get; private set; }

        /// <summary>Fired after the initial weights and CurrentState have been committed.</summary>
        public event Action<AnimationStateDefinition, AnimationStateDefinition> StateChanged;
        /// <summary>Fired after each unpaused Tick, including Tick(0), for a current state.</summary>
        public event Action<AnimationStateDefinition, float> StateUpdated;
        /// <summary>Fired once after an immediate entry or a completed crossfade.</summary>
        public event Action<AnimationStateDefinition> TransitionCompleted;

        public AnimationStateMachine(IAnimationWeightDriver driver)
        {
            this.driver = driver ?? throw new ArgumentNullException(nameof(driver));
        }

        /// <summary>
        /// Validates the entire provider before registering anything. Empty providers are
        /// allowed. A state asset may belong to only one live registration; separate assets
        /// may share a display name. Dispose the returned handle when the provider unloads.
        /// Registration does not prepare or modify the graph.
        /// </summary>
        public IDisposable Register(IAnimationStateProvider provider)
        {
            if (provider == null) throw new ArgumentNullException(nameof(provider));
            IEnumerable<AnimationStateDefinition> definitions = provider.GetAnimationStates();
            if (definitions == null)
                throw new ArgumentException("A provider must return a non-null state sequence.", nameof(provider));

            var states = new List<AnimationStateDefinition>();
            var unique = new HashSet<AnimationStateDefinition>(StateReferenceComparer.Instance);
            foreach (AnimationStateDefinition state in definitions)
            {
                if (state == null)
                    throw new ArgumentException("A provider cannot contain a null state.", nameof(provider));
                if (!unique.Add(state))
                    throw new ArgumentException("A provider cannot contain the same state asset twice.", nameof(provider));
                // CreateSnapshot validates the complete definition, including hidden inputs.
                SnapshotDefinition(state);
                states.Add(state);
            }

            // Recheck the live registry only after user-supplied enumeration/validation ends.
            // A provider must never partially claim states when one of them conflicts.
            foreach (AnimationStateDefinition state in states)
                if (registrations.ContainsKey(state))
                    throw new InvalidOperationException("The state asset is already registered: " + state.StateName);

            var registration = new Registration(this, states.ToArray());
            foreach (AnimationStateDefinition state in states) registrations.Add(state, registration);
            return registration;
        }

        /// <summary>
        /// Reads fresh configuration on every actual entry. Entering the current state is a
        /// no-op unless forceReenter is true. A forced entry reapplies/retargets weights but
        /// deliberately does not rewind clips. CrossFade with duration zero is immediate.
        /// Configuration is validated before Prepare; all driver calls finish before events.
        /// </summary>
        public void ChangeState(AnimationStateDefinition state,
            AnimationStateTransition transition = AnimationStateTransition.Immediate,
            float duration = 0.2f,
            AnimationWeightBlendMode blendMode = AnimationWeightBlendMode.InputWeights,
            bool forceReenter = false)
        {
            EnsureDriverAvailable();
            if (state == null) throw new ArgumentNullException(nameof(state));
            if (!registrations.ContainsKey(state))
                throw new InvalidOperationException("Register this state asset before entering it.");
            ValidateTransition(transition, duration, blendMode);
            if (!forceReenter && ReferenceEquals(CurrentState, state)) return;

            AnimationWeightSnapshot target = SnapshotDefinition(state);
            AnimationWeightSnapshot actual;
            bool fade = transition == AnimationStateTransition.CrossFade && duration > 0;
            driverBusy = true;
            try
            {
                // Prepare only adds/configures cached graph inputs; it must not change the
                // current pose. Capture afterwards includes newly prepared zero-weight inputs.
                driver.Prepare(target.Clone());
                actual = CaptureOwnedSnapshot();
                IncludeMissingKeysAsZero(target, actual);
                // For an interrupted fade, actual is the current graph result, never the
                // source definition or the previous transition's original starting point.
                driver.Apply((fade ? actual : target).Clone());
            }
            finally { driverBusy = false; }

            AnimationStateDefinition previous = CurrentState;
            long ticket = ++generation;
            CurrentState = state;
            IsTransitioning = fade;
            TransitionProgress = fade ? 0 : 1;
            transitionElapsed = 0;
            transitionDuration = fade ? duration : 0;
            transitionBlendMode = blendMode;
            transitionFrom = fade ? actual : null;
            transitionTo = fade ? target : null;

            DispatchChanged(previous, state, ticket);
            if (!fade && IsCurrent(ticket, state)) DispatchCompleted(state, ticket);
        }

        /// <summary>
        /// Advances only the weight transition, then calls the external gameplay update.
        /// Driver animation/event clocks should be advanced separately by their owner.
        /// Pausing freezes both this transition and StateUpdated; explicit commands still work.
        /// Reentrant ChangeState/Clear from business events cancels the old event batch.
        /// </summary>
        public void Tick(float deltaTime)
        {
            ValidateNonNegative(deltaTime, nameof(deltaTime));
            EnsureDriverAvailable();
            if (ticking) throw new InvalidOperationException("Tick cannot be called recursively.");
            if (IsPaused || ReferenceEquals(CurrentState, null)) return;

            ticking = true;
            try
            {
                AnimationStateDefinition state = CurrentState;
                long ticket = generation;
                bool completed = false;
                if (IsTransitioning)
                {
                    double elapsed = Math.Min(transitionElapsed + deltaTime, transitionDuration);
                    float progress = (float)(elapsed / transitionDuration);
                    AnimationWeightSnapshot weights = AnimationWeightSnapshot.Interpolate(
                        transitionFrom, transitionTo, progress, transitionBlendMode);
                    driverBusy = true;
                    try { driver.Apply(weights); }
                    finally { driverBusy = false; }

                    transitionElapsed = elapsed;
                    TransitionProgress = progress;
                    if (elapsed >= transitionDuration)
                    {
                        IsTransitioning = false;
                        transitionFrom = transitionTo = null;
                        completed = true;
                    }
                }

                DispatchUpdated(state, deltaTime, ticket);
                if (completed && IsCurrent(ticket, state)) DispatchCompleted(state, ticket);
            }
            finally { ticking = false; }
        }

        public void Pause() { IsPaused = true; }
        public void Resume() { IsPaused = false; }

        /// <summary>
        /// Cancels entry/transition, zeros every input captured from the driver, and leaves
        /// clip playables cached. Does not resume a paused machine or rewind animation time.
        /// StateChanged(previous, null) is raised only when a state was actually current.
        /// </summary>
        public void Clear()
        {
            EnsureDriverAvailable();
            AnimationStateDefinition previous = ClearCore();
            if (!ReferenceEquals(previous, null)) DispatchChanged(previous, null, generation);
        }

        private AnimationStateDefinition ClearCore()
        {
            driverBusy = true;
            try
            {
                AnimationWeightSnapshot actual = CaptureOwnedSnapshot();
                var zero = new AnimationWeightSnapshot();
                foreach (var item in actual.LayerWeights) zero.SetLayerWeight(item.Key, 0);
                foreach (var item in actual.ClipWeights) zero.SetClipWeight(item.Key, 0);
                driver.Apply(zero);
            }
            finally { driverBusy = false; }

            AnimationStateDefinition previous = CurrentState;
            ++generation;
            CurrentState = null;
            IsTransitioning = false;
            TransitionProgress = 0;
            transitionElapsed = transitionDuration = 0;
            transitionFrom = transitionTo = null;
            return previous;
        }

        private void Unregister(Registration registration)
        {
            EnsureDriverAvailable();
            if (registration.Owner == null) return;
            bool removesCurrent = !ReferenceEquals(CurrentState, null) &&
                Array.Exists(registration.States, state => ReferenceEquals(state, CurrentState));

            // Clear the driver before changing registration ownership. If it fails, the
            // registration is still live and disposal can be retried. Notify business code
            // only after removing the definitions, so callbacks cannot reenter an unload.
            AnimationStateDefinition previous = removesCurrent ? ClearCore() : null;
            foreach (AnimationStateDefinition state in registration.States) registrations.Remove(state);
            registration.Owner = null;
            if (!ReferenceEquals(previous, null)) DispatchChanged(previous, null, generation);
            // A removed source state does not cancel a fade to another registered target.
            // Its captured weights and cached driver inputs remain valid until completion.
        }

        private AnimationWeightSnapshot CaptureOwnedSnapshot()
        {
            AnimationWeightSnapshot snapshot = driver.Capture();
            if (snapshot == null) throw new InvalidOperationException("The driver returned a null snapshot.");
            return snapshot.Clone();
        }

        private static AnimationWeightSnapshot SnapshotDefinition(AnimationStateDefinition state)
        {
            AnimationWeightSnapshot snapshot = state.CreateSnapshot();
            if (snapshot == null) throw new InvalidOperationException("The state returned a null snapshot.");
            return snapshot.Clone();
        }

        private static void IncludeMissingKeysAsZero(AnimationWeightSnapshot target,
            AnimationWeightSnapshot actual)
        {
            foreach (var item in actual.LayerWeights)
                if (!target.LayerWeights.ContainsKey(item.Key)) target.SetLayerWeight(item.Key, 0);
            foreach (var item in actual.ClipWeights)
                if (!target.ClipWeights.ContainsKey(item.Key)) target.SetClipWeight(item.Key, 0);
        }

        private void EnsureDriverAvailable()
        {
            if (driverBusy)
                throw new InvalidOperationException("Do not reenter the state machine from driver operations. Use the state events instead.");
        }

        private bool IsCurrent(long ticket, AnimationStateDefinition state)
        {
            return ticket == generation && ReferenceEquals(CurrentState, state);
        }

        // Events are synchronous and dispatched from a delegate snapshot. Isolate listener
        // exceptions so one gameplay component cannot prevent the other components updating.
        // After a listener changes/clears the state, stop dispatching the stale event batch.
        private void DispatchChanged(AnimationStateDefinition previous, AnimationStateDefinition state, long ticket)
        {
            var handlers = StateChanged;
            if (handlers == null) return;
            foreach (Delegate handler in handlers.GetInvocationList())
            {
                if (!IsCurrent(ticket, state)) break;
                try { ((Action<AnimationStateDefinition, AnimationStateDefinition>)handler)(previous, state); }
                catch (Exception exception) { Debug.LogException(exception); }
            }
        }

        private void DispatchUpdated(AnimationStateDefinition state, float deltaTime, long ticket)
        {
            var handlers = StateUpdated;
            if (handlers == null) return;
            foreach (Delegate handler in handlers.GetInvocationList())
            {
                if (!IsCurrent(ticket, state)) break;
                try { ((Action<AnimationStateDefinition, float>)handler)(state, deltaTime); }
                catch (Exception exception) { Debug.LogException(exception); }
            }
        }

        private void DispatchCompleted(AnimationStateDefinition state, long ticket)
        {
            var handlers = TransitionCompleted;
            if (handlers == null) return;
            foreach (Delegate handler in handlers.GetInvocationList())
            {
                if (!IsCurrent(ticket, state)) break;
                try { ((Action<AnimationStateDefinition>)handler)(state); }
                catch (Exception exception) { Debug.LogException(exception); }
            }
        }

        private static void ValidateTransition(AnimationStateTransition transition, float duration,
            AnimationWeightBlendMode blendMode)
        {
            if (transition != AnimationStateTransition.Immediate && transition != AnimationStateTransition.CrossFade)
                throw new ArgumentOutOfRangeException(nameof(transition));
            ValidateNonNegative(duration, nameof(duration));
            if (blendMode != AnimationWeightBlendMode.InputWeights && blendMode != AnimationWeightBlendMode.EffectiveContribution)
                throw new ArgumentOutOfRangeException(nameof(blendMode));
        }

        private static void ValidateNonNegative(float value, string name)
        {
            if (float.IsNaN(value) || float.IsInfinity(value) || value < 0)
                throw new ArgumentOutOfRangeException(name, "Value must be finite and non-negative.");
        }

        private sealed class Registration : IDisposable
        {
            internal AnimationStateMachine Owner;
            internal readonly AnimationStateDefinition[] States;

            internal Registration(AnimationStateMachine owner, AnimationStateDefinition[] states)
            {
                Owner = owner;
                States = states;
            }

            public void Dispose()
            {
                AnimationStateMachine owner = Owner;
                if (owner != null) owner.Unregister(this);
            }
        }

        private sealed class StateReferenceComparer : IEqualityComparer<AnimationStateDefinition>
        {
            internal static readonly StateReferenceComparer Instance = new StateReferenceComparer();
            public bool Equals(AnimationStateDefinition x, AnimationStateDefinition y) { return ReferenceEquals(x, y); }
            public int GetHashCode(AnimationStateDefinition state) { return RuntimeHelpers.GetHashCode(state); }
        }
    }
}
