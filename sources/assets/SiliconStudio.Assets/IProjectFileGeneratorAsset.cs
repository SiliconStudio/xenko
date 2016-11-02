namespace SiliconStudio.Assets
{
    /// <summary>
    /// An asset that generates another file.
    /// </summary>
    public interface IProjectFileGeneratorAsset : IProjectAsset
    {
        string Generator { get; }

        void SaveGeneratedAsset(AssetItem assetItem);
    }
}