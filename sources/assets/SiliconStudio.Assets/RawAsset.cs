// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System.ComponentModel;

using SiliconStudio.Assets.Compiler;
using SiliconStudio.Core;

namespace SiliconStudio.Assets
{
    /// <summary>
    /// A raw asset, an asset that is imported as-is.
    /// </summary>
    /// <userdoc>A raw asset, an asset that is imported as-is.</userdoc>
    [DataContract("RawAsset")]
    [AssetDescription(FileExtension)]
    [Display(1050, "Raw Asset")]
    public sealed class RawAsset : AssetWithSource
    {
        public const string FileExtension = ".xkraw";

        /// <summary>
        /// Initializes a new instance of the <see cref="RawAsset"/> class.
        /// </summary>
        public RawAsset()
        {
            Compress = true;
        }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="RawAsset"/> will be compressed when compiled.
        /// </summary>
        /// <value><c>true</c> if this asset will be compressed when compiled; otherwise, <c>false</c>.</value>
        /// <userdoc>A boolean indicating whether this asset will be compressed when compiled</userdoc>
        [DefaultValue(true)]
        public bool Compress { get; set; }
    }
}
