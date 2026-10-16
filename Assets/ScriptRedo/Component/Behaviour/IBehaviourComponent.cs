namespace ScriptRedo
{
    public interface IBehaviourComponent<in T> : IComponent where T : IBehaviour
    {
        void ChangeBehaviour(T behaviourToChange);
    }
}