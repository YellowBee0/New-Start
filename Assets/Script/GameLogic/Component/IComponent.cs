namespace YBFramework.GameLogic.Component
{
    public interface IComponent
    {
        Entity GetOwner();

        void SetOwner(Entity entity);

        void OnAdd();

        void OnRemove();
    }
}