namespace ScriptRedo
{
    public interface ISerializableData<out TRuntime>
    {
        TRuntime CreateRuntimeInstance();
    }
}