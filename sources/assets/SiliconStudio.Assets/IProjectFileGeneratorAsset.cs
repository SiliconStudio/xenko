namespace SiliconStudio.Assets
{
    /// <summary>
    /// An asset that generates another file.
    /// </summary>
    public interface IProjectFileGeneratorAsset : IProjectAsset
    {
        string Generator { get; }

        string GeneratedAbsolutePath { get; set; }

        string GeneratedInclude { get; set; }

        void SaveGeneratedAsset();
    }
}