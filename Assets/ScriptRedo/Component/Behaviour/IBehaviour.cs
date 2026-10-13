namespace ScriptRedo
{
    public interface IBehaviour
    {
        void OnAddToEntity(Entity entity);
        
        void OnRemoveFromEntity(Entity entity);

        bool IsRunning();
        
        /// <summary>
        /// 开始执行行为，不会从初始状态开始执行，如果想从初始状态执行需要调用一次Reset
        /// </summary>
        void Start();
        
        /// <summary>
        /// 停止执行行为，不会重置
        /// </summary>
        void Stop();

        /// <summary>
        /// 重置行为的状态。以后想要实现行为切换或者改变，比如A的行为和B的行为进行交换或者A改变另外的行为，但是对于一些运行过的行为多少会保留一些运行状态，
        /// 这些状态如果直接切换到其他对象上，很可能会导致无法预料的错误，所以目前准备直接重置状态然后再执行切换。
        /// </summary>
        void Reset();
    }
}