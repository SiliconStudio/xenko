// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Assets;
using SiliconStudio.Assets.Compiler;
using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Paradox.Assets.Materials;
using SiliconStudio.Paradox.Graphics;

namespace SiliconStudio.Paradox.Assets.GeometricPrimitives
{
    /// <summary>
    /// The geometric primitive asset.
    /// </summary>
    [DataContract("GeometricPrimitiveAsset")]
    [AssetFileExtension(FileExtension)]
    //[ThumbnailCompiler(PreviewerCompilerNames.MaterialThumbnailCompilerQualifiedName, true)]
    [AssetCompiler(typeof(GeometricPrimitiveAssetCompiler))]
    [Display("3D Primitive", "A 3D primitive asset")]
    public sealed class GeometricPrimitiveAsset : Asset
    {
        /// <summary>
        /// The default file extension used by the <see cref="GeometricPrimitiveAsset"/>.
        /// </summary>
        public const string FileExtension = ".pdxgeo";

        /// <summary>
        /// Initializes a new instance of the <see cref="GeometricPrimitiveAsset"/> class.
        /// </summary>
        public GeometricPrimitiveAsset()
        {
            Model = new GeometricPrimitive.Cube.Model();
        }

        /// <summary>
        /// Gets or sets the type of geometric primitive.
        /// </summary>
        /// <value>The type of geometric primitive.</value>
        [DataMember(10)]
        [NotNull]
        [Display("Type")]
        public IGeometricPrimitiveModel Model { get; set; }

        /// <summary>
        /// Gets or sets the material.
        /// </summary>
        /// <value>The material.</value>
        [DataMember(20)]
        [NotNull]
        [Display("Material")]
        public AssetReference<MaterialAsset> Material { get; set; }
    }
}