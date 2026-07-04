namespace YBFramework.Bridge
{
    /// <summary>
    /// 定义序列化数据与其运行时类型之间的创建契约。
    /// </summary>
    public interface IRuntimeData<out TRuntime>
    {
        TRuntime CreateRuntimeInstance();
    }
}