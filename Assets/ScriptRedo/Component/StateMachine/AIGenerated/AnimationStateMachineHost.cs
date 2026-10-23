using System;
using UnityEngine;

namespace DistributedAnimationStates
{
    /// <summary>Scene clock/lifetime adapter. Contains no state definitions or transition rules.</summary>
    [DisallowMultipleComponent, RequireComponent(typeof(PlayableAnimationWeightDriver))]
    public sealed class AnimationStateMachineHost : MonoBehaviour
    {
        public PlayableAnimationWeightDriver Driver;
        public bool AutoUpdate = true;
        public bool UseUnscaledTime;
        private AnimationStateMachine machine;
        private bool ticking;

        public AnimationStateMachine Machine
        {
            get
            {
                if (machine == null)
                {
                    if (Driver == null) Driver = GetComponent<PlayableAnimationWeightDriver>();
                    if (Driver == null) throw new InvalidOperationException("A weight driver is required.");
                    Driver.Initialize();
                    machine = new AnimationStateMachine(Driver);
                }
                return machine;
            }
        }

        public void ChangeState(AnimationStateDefinition state,
            AnimationStateTransition transition = AnimationStateTransition.Immediate,
            float duration = 0.2f,
            AnimationWeightBlendMode blendMode = AnimationWeightBlendMode.InputWeights,
            bool forceReenter = false)
        {
            Machine.ChangeState(state, transition, duration, blendMode, forceReenter);
        }

        /// <summary>Set AutoUpdate=false for an entity system clock. Call exactly once per frame.</summary>
        public void Tick(float deltaTime)
        {
            if (float.IsNaN(deltaTime) || float.IsInfinity(deltaTime) || deltaTime < 0)
                throw new ArgumentOutOfRangeException(nameof(deltaTime));
            if (ticking) throw new InvalidOperationException("Host Tick cannot be called recursively.");
            if (this == null || !isActiveAndEnabled) return;
            if (machine != null && (Driver == null || !Driver.isActiveAndEnabled)) return;
            var currentMachine = Machine;
            ticking = true;
            try
            {
                currentMachine.Tick(deltaTime);
                // Gameplay callbacks may disable or destroy the character/driver.
                if (this == null || !isActiveAndEnabled || Driver == null || !Driver.isActiveAndEnabled) return;
                if (currentMachine.IsPaused) Driver.SampleIfDirty();
                else Driver.Tick(deltaTime);
            }
            finally { ticking = false; }
        }

        public void Pause() { Machine.Pause(); Driver.Pause(); }
        public void Resume() { Machine.Resume(); Driver.Resume(); }
        public void Replay() { _ = Machine; Driver.Replay(); }
        public void ReplayState()
        {
            var state = Machine.CurrentState;
            if (state != null) Driver.Replay(state.CreateSnapshot());
        }

        private void Update()
        {
            if (AutoUpdate && machine != null)
                Tick(UseUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime);
        }
    }
}
