namespace ScriptRedo
{
    public interface IAnimationEventProvider
    {
        string GetName();

        IAnimationEventInvoker CreateInvoker(string eventName, IRuntimeData<IAnimationEventInvoker> eventArgs);
    }
}