using System;

namespace SiliconStudio.Assets
{
    /// <summary>
    /// Describes what format version this asset currently uses, for asset upgrading.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class AssetFormatVersionAttribute : Attribute
    {
        public AssetFormatVersionAttribute(int version, params Type[] assetUpdaterTypes)
        {
            Version = version;
            AssetUpdaterTypes = assetUpdaterTypes;
        }

        /// <summary>
        /// Gets the current format version of this asset.
        /// </summary>
        /// <value>
        /// The current format version of this asset.
        /// </value>
        public int Version { get; private set; }

        public Type[] AssetUpdaterTypes { get; private set; }
    }
}