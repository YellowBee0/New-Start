namespace ScriptRedo
{
    public interface IComponent
    {
        //实现IComponent的类一般都需要一个InitializeData(对应的IComponentData（这里指的是准确的类型）)函数初始化数据。
        //这个函数在newIComponent实例调用一次，如果IComponent实例可以复用（作为对象池对象），那么可以多次调用，不过要注意资源释放

        /// <summary>
        /// 当添加到Entity时调用
        /// </summary>
        /// <param name="entity">添加的Entity</param>
        void OnAddToEntity(Entity entity);

        /// <summary>
        /// 当从Entity移除时调用
        /// </summary>
        /// <param name="entity">从那个Entity移除</param>
        void OnRemoveFromEntity(Entity entity);
    }
}