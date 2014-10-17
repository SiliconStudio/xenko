// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;

namespace SiliconStudio.Assets
{
    /// <summary>
    /// Associates user-friendly names and descriptions to an asset type.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class AssetDescriptionAttribute : Attribute
    {
        private readonly AssetDescription assetDescription;

        /// <summary>
        /// Gets the display name of the asset
        /// </summary>
        public string DisplayName { get { return assetDescription.DisplayName; } }

        /// <summary>
        /// Gets a description of the asset.
        /// </summary>
        public string Description { get { return assetDescription.Description; } }

        /// <summary>
        /// Gets whether the thumbnails of the asset type are dynamic and should be regenerated each time a property changes.
        /// </summary>
        public bool DynamicThumbnails { get { return assetDescription.DynamicThumbnails; } }

        /// <summary>
        /// Initializes a new instance of the <see cref="AssetDescriptionAttribute"/> class.
        /// </summary>
        /// <param name="displayName">A user-friendly name describing the asset type.</param>
        /// <param name="description">A sentence describing the purpose of the asset type.</param>
        /// <param name="dynamicThumbnails">Indicates that the thumbnails of the asset type are dynamic and should be regenerated each time a property changes.</param>
        public AssetDescriptionAttribute(string displayName, string description, bool dynamicThumbnails)
        {
            assetDescription = new AssetDescription(displayName, description, dynamicThumbnails);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AssetDescriptionAttribute"/> class.
        /// </summary>
        /// <param name="displayName">A user-friendly name describing the asset type.</param>
        /// <param name="description">A sentence describing the purpose of the asset type.</param>
        public AssetDescriptionAttribute(string displayName, string description)
            : this(displayName, description, false)
        {
        }

        internal AssetDescription GetDescription()
        {
            return assetDescription;
        }
    }
}
