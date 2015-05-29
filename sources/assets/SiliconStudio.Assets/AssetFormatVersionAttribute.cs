using System;

namespace SiliconStudio.Assets
{
    /// <summary>
    /// Describes what format version this asset currently uses, for asset upgrading.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class AssetFormatVersionAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AssetFormatVersionAttribute"/> class.
        /// </summary>
        /// <param name="version">The current format version of this asset.</param>
        /// <param name="minUpgradableVersion">The minimum format version that supports upgrade for this asset.</param>
        public AssetFormatVersionAttribute(int version, int minUpgradableVersion = 0)
        {
            Version = version;
            MinUpgradableVersion = minUpgradableVersion;
        }

        /// <summary>
        /// Gets the current format version of this asset.
        /// </summary>
        /// <value>
        /// The current format version of this asset.
        /// </value>
        public int Version { get; set; }

        /// <summary>
        /// Gets the minimum format version that supports upgrade for this asset.
        /// </summary>
        /// <value>
        /// The minimum format version that supports upgrade for this asset.
        /// </value>
        public int MinUpgradableVersion { get; set; }
    }
}