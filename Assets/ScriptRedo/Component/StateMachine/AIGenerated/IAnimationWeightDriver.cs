using System.Collections.Generic;

namespace DistributedAnimationStates
{
    public interface IAnimationStateProvider
    {
        IEnumerable<AnimationStateDefinition> GetAnimationStates();
    }

    /// <summary>
    /// Adapt an existing graph here. Apply must validate before mutation, set omitted inputs to zero,
    /// apply the full frame without evaluating between individual writes, and never start its own fade.
    /// Prepare may cache zero-weight clip inputs but must preserve all currently visible weights/times.
    /// Capture must return current raw graph weights, including inputs from interrupted transitions.
    /// </summary>
    public interface IAnimationWeightDriver
    {
        void Prepare(AnimationWeightSnapshot target);
        AnimationWeightSnapshot Capture();
        void Apply(AnimationWeightSnapshot weights);
    }
}
