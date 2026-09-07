#if UNITY_EDITOR
namespace YBFramework.Bridge.Editor
{
    public interface IFieldPath
    {
        string GetFieldPath();

        void SetFieldPath(string fieldPath);
    }
}
#endif