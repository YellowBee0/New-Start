namespace ScriptRedo
{
    public interface IBuffBehaviour : IBehaviour
    {
        void OnAddToBuffManager(BuffManagerComponent buffManager);

        void OnRemoveFromBuffManager(BuffManagerComponent buffManager);
    }
}