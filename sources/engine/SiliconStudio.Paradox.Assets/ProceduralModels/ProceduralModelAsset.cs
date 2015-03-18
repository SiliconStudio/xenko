// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Assets;
using SiliconStudio.Assets.Compiler;
using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Paradox.Effects.ProceduralModels;
using SiliconStudio.Paradox.Engine.Graphics.ProceduralModels;

namespace SiliconStudio.Paradox.Assets.ProceduralModels
{
    /// <summary>
    /// The geometric primitive asset.
    /// </summary>
    [DataContract("ProceduralModelAsset")]
    [AssetFileExtension(FileExtension)]
    //[ThumbnailCompiler(PreviewerCompilerNames.MaterialThumbnailCompilerQualifiedName, true)] // TODO: Add preview
    [AssetCompiler(typeof(ProceduralModelAssetCompiler))]
    [Display("Procedural Model", "A procedural model")]
    public sealed class ProceduralModelAsset : Asset
    {
        /// <summary>
        /// The default file extension used by the <see cref="ProceduralModelAsset"/>.
        /// </summary>
        public const string FileExtension = ".pdxpromodel";

        /// <summary>
        /// Initializes a new instance of the <see cref="ProceduralModelAsset"/> class.
        /// </summary>
        public ProceduralModelAsset()
        {
            Type = new CubeProceduralModel();
        }

        /// <summary>
        /// Gets or sets the type.
        /// </summary>
        /// <value>The type.</value>
        [DataMember(10)]
        [NotNull]
        [Display("Type", AlwaysExpand = true)]
        public IProceduralModel Type { get; set; }
    }
}