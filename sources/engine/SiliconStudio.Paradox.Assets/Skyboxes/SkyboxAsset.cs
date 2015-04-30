// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.ComponentModel;

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
    [AssetDescription(FileExtension)]
    //[ThumbnailCompiler(PreviewerCompilerNames.MaterialThumbnailCompilerQualifiedName, true)]
    [AssetCompiler(typeof(SkyboxAssetCompiler))]
    [Display(100, "Skybox", "A skybox asset")]
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
            DiffuseSHOrder = SkyboxPreFilteringDiffuseOrder.Order3;
            SpecularCubeMapSize = 256;
        }

        protected override int InternalBuildOrder
        {
            get { return 500; }
        }

        /// <summary>
        /// Gets or sets the type of skybox.
        /// </summary>
        /// <value>The type of skybox.</value>
        [DataMember(10)]
        [NotNull]
        [Display("Type", AlwaysExpand = true)]
        public ISkyboxModel Model { get; set; }

        /// <summary>
        /// Gets or sets the diffuse sh order.
        /// </summary>
        /// <value>The diffuse sh order.</value>
        [DefaultValue(SkyboxPreFilteringDiffuseOrder.Order3)]
        [Display("Diffuse SH Order")]
        [DataMember(20)]
        public SkyboxPreFilteringDiffuseOrder DiffuseSHOrder { get; set; }

        /// <summary>
        /// Gets or sets the diffuse sh order.
        /// </summary>
        /// <value>The diffuse sh order.</value>
        [DefaultValue(256)]
        [Display("Specular CubeMap Size")]
        [DataMember(30)]
        public int SpecularCubeMapSize { get; set; }
    }
}