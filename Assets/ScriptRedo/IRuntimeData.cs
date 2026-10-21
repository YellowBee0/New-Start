namespace ScriptRedo
{
    //限制：一份数据只能创建一种运行时实例
    public interface IRuntimeData<out TRuntime>
    {
        TRuntime CreateRuntimeInstance();
    }
}