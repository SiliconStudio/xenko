// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
namespace SiliconStudio.Assets
{
    /// <summary>
    /// Contains user-friendly names and descriptions of an asset type.
    /// </summary>
    public class AssetDescription
    {
        /// <summary>
        /// Gets the display name of the asset
        /// </summary>
        public string DisplayName { get; private set; }

        /// <summary>
        /// Gets a description of the asset.
        /// </summary>
        public string Description { get; private set; }

        /// <summary>
        /// Gets whether the thumbnails of the asset type are dynamic and should be regenerated each time a property changes.
        /// </summary>
        public bool DynamicThumbnails { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AssetDescription"/> class.
        /// </summary>
        /// <param name="displayName">A user-friendly name describing the asset type.</param>
        /// <param name="description">A sentence describing the purpose of the asset type.</param>
        /// <param name="dynamicThumbnails"></param>
        public AssetDescription(string displayName, string description, bool dynamicThumbnails)
        {
            DisplayName = displayName;
            Description = description;
            DynamicThumbnails = dynamicThumbnails;
        }
    }
}
