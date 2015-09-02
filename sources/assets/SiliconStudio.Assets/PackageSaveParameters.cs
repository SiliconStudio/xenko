namespace SiliconStudio.Assets
{
    public class PackageSaveParameters
    {
        private static readonly PackageSaveParameters DefaultParameters = new PackageSaveParameters();

        public static PackageSaveParameters Default()
        {
            return DefaultParameters.Clone();
        }

        /// <summary>
        /// Gets or sets the behavior when dealing with asset having <see cref="AssetImport.SourceKeepSideBySide"/> enabled.
        /// </summary>
        public PackageSaveSourceFileOperations SaveSourceFileOperations { get; set; } = PackageSaveSourceFileOperations.None;

        /// <summary>
        /// Clones this instance.
        /// </summary>
        /// <returns>PackageLoadParameters.</returns>
        public PackageSaveParameters Clone()
        {
            return (PackageSaveParameters)MemberwiseClone();
        }
    }
}