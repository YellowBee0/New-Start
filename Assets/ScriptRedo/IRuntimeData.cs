namespace ScriptRedo
{
    public interface IRuntimeData<out TRuntime>
    {
        TRuntime CreateRuntimeInstance();
    }
}