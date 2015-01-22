// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Assets;
using SiliconStudio.Assets.Compiler;
using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;

namespace SiliconStudio.Paradox.Assets.Skyboxes
{
    /// <summary>
    /// The skybox asset.
    /// </summary>
    [DataContract("SkyboxAsset")]
    [AssetFileExtension(FileExtension)]
    //[ThumbnailCompiler(PreviewerCompilerNames.MaterialThumbnailCompilerQualifiedName, true)]
    [AssetCompiler(typeof(SkyboxAssetCompiler))]
    [Display("Skybox", "A skybox asset")]
    public sealed class SkyboxAsset : Asset
    {
        /// <summary>
        /// The default file extension used by the <see cref="SkyboxAsset"/>.
        /// </summary>
        public const string FileExtension = ".pdxsky";

        /// <summary>
        /// Initializes a new instance of the <see cref="SkyboxAsset"/> class.
        /// </summary>
        public SkyboxAsset()
        {
            Model = new SkyboxCubeMapModel();
        }

        /// <summary>
        /// Gets or sets the type of skybox.
        /// </summary>
        /// <value>The type of skybox.</value>
        [DataMember(10)]
        [NotNull]
        [Display("Type")]
        public ISkyboxModel Model { get; set; }

        // TODO: Add prefiltering options...etc.
    }
}