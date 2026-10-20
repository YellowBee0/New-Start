using System;

namespace ScriptRedo
{
    [Serializable]
    public sealed class AnimationPlayableGraphData : IRuntimeData<AnimationPlayableGraph>
    {
        public string AnimatorPath;

        //有几个Layer，每个Layer有AvatarMask、覆盖类型和有哪些端口
        public int Layer;

        public AnimationPlayableGraph CreateRuntimeInstance()
        {
            AnimationPlayableGraph animationPlayableGraph = new AnimationPlayableGraph();
            animationPlayableGraph.InitializeData(this);
            return animationPlayableGraph;
        }
    }
}