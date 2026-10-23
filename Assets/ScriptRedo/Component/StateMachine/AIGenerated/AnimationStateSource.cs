using System;
using System.Collections.Generic;
using UnityEngine;

namespace DistributedAnimationStates
{
    /// <summary>
    /// Convenience provider for a weapon/prefab/module. Other entity components can implement
    /// IAnimationStateProvider themselves; they do not need to inherit from this component.
    /// </summary>
    public sealed class AnimationStateSource : MonoBehaviour, IAnimationStateProvider
    {
        public AnimationStateMachineHost Host;
        public AnimationStateDefinition[] States = new AnimationStateDefinition[0];
        [Tooltip("Optional initial state owned by this provider. Leave empty for equipment that should only register.")]
        public AnimationStateDefinition InitialState;
        private IDisposable registration;

        public IEnumerable<AnimationStateDefinition> GetAnimationStates() { return States; }

        public void Register()
        {
            if (registration != null) return;
            if (Host == null) Host = GetComponentInParent<AnimationStateMachineHost>();
            if (Host == null) throw new InvalidOperationException("Assign an AnimationStateMachineHost.");
            if (InitialState != null && (States == null || Array.IndexOf(States, InitialState) < 0))
                throw new ArgumentException("InitialState must belong to this source's States list.");
            registration = Host.Machine.Register(this);
            try
            {
                if (InitialState != null) Host.ChangeState(InitialState);
            }
            catch
            {
                registration.Dispose();
                registration = null;
                throw;
            }
        }

        public void Unregister()
        {
            // If the whole character has already been destroyed, there is no live graph to clear.
            // Keep the handle if a backend error makes disposal fail, so it can be retried.
            if (Host != null) registration?.Dispose();
            registration = null;
        }

        private void OnEnable() { Register(); }
        private void OnDisable() { Unregister(); }
    }
}
