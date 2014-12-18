// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.ComponentModel;

using SiliconStudio.Assets;
using SiliconStudio.Assets.Compiler;
using SiliconStudio.Core;
using SiliconStudio.Core.IO;
using SiliconStudio.Core.Reflection;

namespace SiliconStudio.Paradox.Assets.Materials
{
    /// <summary>
    /// Description of a material.
    /// </summary>
    [DataContract("MaterialAsset")]
    [AssetFileExtension(FileExtension)]
    [ThumbnailCompiler(PreviewerCompilerNames.MaterialThumbnailCompilerQualifiedName, true)]
    [AssetCompiler(typeof(MaterialAssetCompiler))]
    [ObjectFactory(typeof(MaterialFactory))]
    [Display("Material", "A material")]
    public sealed class MaterialAssetOld : Asset
    {
        /// <summary>
        /// The default file extension used by the <see cref="MaterialAssetOld"/>.
        /// </summary>
        public const string FileExtension = ".pdxmat";

        /// <summary>
        /// Initializes a new instance of the <see cref="MaterialAssetOld"/> class.
        /// </summary>
        public MaterialAssetOld()
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

        private class MaterialFactory : IObjectFactory
        {
            public object New(Type type)
            {
                var newMaterial = new MaterialAssetOld { Material = new MaterialDescription() };
                return newMaterial;
            }
        }
    }
}
