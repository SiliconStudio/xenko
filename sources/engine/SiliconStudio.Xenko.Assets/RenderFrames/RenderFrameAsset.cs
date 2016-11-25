// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.ComponentModel;

using SiliconStudio.Assets;
using SiliconStudio.Assets.Compiler;
using SiliconStudio.Core;
using SiliconStudio.Xenko.Rendering;

namespace SiliconStudio.Xenko.Assets.RenderFrames
{
    /// <summary>
    /// Describes a texture asset.
    /// </summary>
    [DataContract("RenderFrame")]
    [AssetDescription(FileExtension)]
    [AssetContentType(typeof(RenderFrame))]
    [AssetCompiler(typeof(RenderFrameAssetCompiler))]
    [Display(1100, "Render Frame")]
    public class RenderFrameAsset : Asset
    {
        /// <summary>
        /// The default file extension used by the <see cref="RenderFrameAsset"/>.
        /// </summary>
        public const string FileExtension = ".xkframe;.pdxframe";

        /// <summary>
        /// Gets or sets the descriptor of the render frame.
        /// </summary>
        /// <value>The descriptor.</value>
        /// <userdoc>The description of the render frame</userdoc>
        [DataMember(10)]
        [Display("Description")]
        [Category]
        public RenderFrameDescriptor Descriptor;

        public RenderFrameAsset()
        {
            Descriptor = RenderFrameDescriptor.Default();
        }
    }
}