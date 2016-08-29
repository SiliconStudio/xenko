namespace SiliconStudio.Assets
{
    public interface IAssetWithLocation
    {
        /// <summary>
        /// Gets or sets the absolute source location of this asset on the disk.
        /// </summary>
        /// <value>The absolute source location.</value>
        string AbsoluteSourceLocation { get; set; }
    }
}