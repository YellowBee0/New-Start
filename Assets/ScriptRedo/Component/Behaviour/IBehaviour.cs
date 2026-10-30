namespace ScriptRedo
{
    public interface IBehaviour
    {
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
        /// 重置行为的状态。只能重置状态不能改变行为的运行状态，即Start/Stop行为。
        /// 重置不代表会Start/Stop行为，如果行为在运行状态，调用Reset只会重置某些缓存数据
        /// </summary>
        void Reset();
    }
}