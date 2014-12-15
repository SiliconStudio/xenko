// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.ComponentModel;

using SiliconStudio.Assets;
using SiliconStudio.Assets.Compiler;
using SiliconStudio.Core;
using SiliconStudio.Core.IO;

namespace SiliconStudio.Paradox.Assets.Materials
{
    /// <summary>
    /// Description of a material.
    /// </summary>
    [DataContract("MaterialAsset")]
    [AssetFileExtension(FileExtension)]
    [ThumbnailCompiler(PreviewerCompilerNames.MaterialThumbnailCompilerQualifiedName, true)]
    [AssetCompiler(typeof(MaterialAssetCompiler))]
    [AssetFactory(typeof(MaterialFactory))]
    [Display("Material", "A material")]
    public sealed class MaterialAsset : Asset
    {
        /// <summary>
        /// The default file extension used by the <see cref="MaterialAsset"/>.
        /// </summary>
        public const string FileExtension = ".pdxmat";

        /// <summary>
        /// Initializes a new instance of the <see cref="MaterialAsset"/> class.
        /// </summary>
        public MaterialAsset()
        {
            BuildOrder = 250;
        }

        [Obsolete]
        [DataMember(10)]
        [DefaultValue(null)]
        [Browsable(false)]
        public UFile Source { get; set; }

        /// <summary>
        /// The material.
        /// </summary>
        [DataMember(20)]
        public MaterialDescription Material { get; set; }

        private class MaterialFactory : IAssetFactory
        {
            public Asset New()
            {
                var newMaterial = new MaterialAsset { Material = new MaterialDescription() };
                return newMaterial;
            }
        }
    }
}
