namespace ScriptRedo
{
    public interface IState
    {
        void Enter();

        void Exit();

        void Update(float deltaTime);
    }
}