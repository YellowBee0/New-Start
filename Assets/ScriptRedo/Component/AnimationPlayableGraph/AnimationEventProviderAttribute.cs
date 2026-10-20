#if UNITY_EDITOR
using System;

namespace ScriptRedo
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public sealed class AnimationEventProviderAttribute : Attribute
    {
        /// <summary>
        /// 动画事件提供者名。
        /// 这个参数等于IAnimationEventProvider.GetName。设计这个特性是为了在编辑器模式下编辑动画事件的时候可以获取到对应的事件，运行时通过接口获取名字匹配
        /// </summary>
        public readonly string ProviderName;

        /// <summary>
        /// 是否替换父类的事件。
        /// 当父类实现IAnimationEventProvider，且标记CreateInvoker为虚函数，并有自己的实现，就需要设置这个参数为Replace还是Merge，同时父类GetName也得为虚函数
        /// </summary>
        public readonly ProviderAnimationEventInheritance Inheritance;

        public AnimationEventProviderAttribute(string providerName, ProviderAnimationEventInheritance inheritance)
        {
            ProviderName = providerName;
            Inheritance = inheritance;
        }
    }
}
#endif