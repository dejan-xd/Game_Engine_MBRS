using Editor.Content;

namespace Editor.Editors
{
    interface IAssetEditor
    {
        Asset Asset { get; }

        void SetAsset(AssetInfo asset);
    }
}