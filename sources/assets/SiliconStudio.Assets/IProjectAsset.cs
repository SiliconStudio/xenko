using SiliconStudio.Core;

namespace SiliconStudio.Assets
{
    /// <summary>
    /// An asset that is stored in a project file (such as .csproj).
    /// </summary>
    public interface IProjectAsset : IAssetWithLocation
    {
        /// <summary>
        /// Gets or sets the absolute project (csproj) location of this asset on the disk.
        /// </summary>
        /// <value>The absolute source location.</value>
        string AbsoluteProjectLocation { get; set; }

        string ProjectInclude { get; set; }

        string ProjectName { get; set; }
    }
}