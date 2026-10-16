using UnityEngine;

namespace ScriptRedo
{
    public enum BuffType
    {
        [InspectorName("增益")] Buff,

        [InspectorName("中性")] Neutral,

        [InspectorName("减益")] DeBuff
    }
}