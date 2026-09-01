namespace YBFramework.Editor.Graph
{
    public interface IGraphAssetChangeListener
    {
        void OnAddGraphAsset(string graphAssetPath);

        void OnRemoveGraphAsset(string graphAssetPath);
    }
}