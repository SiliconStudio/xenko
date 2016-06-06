namespace SiliconStudio.Assets.Tracking
{
    /// <summary>
    /// Describes a change related to the source files used by an asset.
    /// </summary>
    public enum SourceFileChangeType
    {
        /// <summary>
        /// The change occurred in an asset that now has a different list of source files.
        /// </summary>
        Asset,
        /// <summary>
        /// The change occurred in an source file that has been modified externally.
        /// </summary>
        SourceFile
    }
}
