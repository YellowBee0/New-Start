#if UNITY_EDITOR
namespace ScriptRedo
{
    public enum ProviderAnimationEventInheritance
    {
        /// <summary>
        /// 没有继承
        /// </summary>
        None,

        /// <summary>
        /// 替换父类的事件
        /// </summary>
        Replace,

        /// <summary>
        /// 合并父类的事件
        /// </summary>
        Merge,
    }
}
#endif