using System;

namespace SiliconStudio.Assets
{
    /// <summary>
    /// Describes which upgrader type to use to upgrade an asset, depending on this current version number.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class AssetUpgraderAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AssetUpgraderAttribute"/> with a range of supported initial version numbers.
        /// </summary>
        /// <param name="startMinVersion">The minimal initial version number this upgrader can work on.</param>
        /// <param name="startMaxVersion">The maximal initial version number this upgrader can work on.</param>
        /// <param name="targetVersion">The target version number of this upgrader.</param>
        /// <param name="assetUpgraderType">The type of upgrader to instantiate to upgrade the asset.</param>
        public AssetUpgraderAttribute(int startMinVersion, int startMaxVersion, int targetVersion, Type assetUpgraderType)
        {
            if (!typeof(IAssetUpgrader).IsAssignableFrom(assetUpgraderType))
                throw new ArgumentException(@"The assetUpgraderType must implement IAssetUpgrader interface", "assetUpgraderType");
            if (startMaxVersion < startMinVersion)
                throw new ArgumentException(@"The maximal start version is lower than the minimal start version.", "startMaxVersion");
            if (targetVersion <= startMaxVersion)
                throw new ArgumentException(@"The target version is lower or equal to the maximal start version.", "targetVersion");
            StartMinVersion = startMinVersion;
            StartMaxVersion = startMaxVersion;
            TargetVersion = targetVersion;
            AssetUpgraderType = assetUpgraderType;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AssetUpgraderAttribute"/> with a single supported initial version number.
        /// </summary>
        /// <param name="startVersion">The initial version number this upgrader can work on.</param>
        /// <param name="targetVersion">The target version number of this upgrader.</param>
        /// <param name="assetUpgraderType">The type of upgrader to instantiate to upgrade the asset.</param>
        public AssetUpgraderAttribute(int startVersion, int targetVersion, Type assetUpgraderType)
            : this(startVersion, startVersion, targetVersion, assetUpgraderType)
        {
        }

        /// <summary>
        /// Gets or sets the minimal initial version number this upgrader can work on.
        /// </summary>
        public int StartMinVersion { get; set; }

        /// <summary>
        /// Gets or sets the maximal initial version number this upgrader can work on.
        /// </summary>
        public int StartMaxVersion { get; set; }

        /// <summary>
        /// Gets or sets the target version number of this upgrader.
        /// </summary>
        public int TargetVersion { get; set; }

        /// <summary>
        /// Gets or sets the type of upgrader to instantiate to upgrade the asset.
        /// </summary>
        public Type AssetUpgraderType { get; set; }
    }
}