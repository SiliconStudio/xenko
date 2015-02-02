// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.ComponentModel;

using SiliconStudio.Assets;
using SiliconStudio.Assets.Compiler;
using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Core.Reflection;
using SiliconStudio.Paradox.Effects;
using SiliconStudio.Paradox.Effects.Data;

namespace SiliconStudio.Paradox.Assets.Materials
{
    /// <summary>
    /// The material asset.
    /// </summary>
    [DataContract("MaterialAsset")]
    [AssetFileExtension(FileExtension)]
    [ThumbnailCompiler(PreviewerCompilerNames.MaterialThumbnailCompilerQualifiedName, true)]
    [AssetCompiler(typeof(MaterialAssetCompiler))]
    [ObjectFactory(typeof(MaterialFactory))]
    [Display("Material", "A material")]
    public sealed class MaterialAsset : Asset, IMaterialShaderGenerator
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
            Parameters = new ParameterCollection();
            Attributes = new MaterialAttributes();
            Layers = new MaterialBlendLayers();
            //Overrides = new Dictionary<string, ComputeNode>();
        }

        /// <summary>
        /// Gets or sets the material attributes.
        /// </summary>
        /// <value>The material attributes.</value>
        [DataMember(10)]
        [NotNull]
        [Display("Attributes", AlwaysExpand = true)]
        public MaterialAttributes Attributes { get; set; }


        /// <summary>
        /// Gets or sets the material compositor.
        /// </summary>
        /// <value>The material compositor.</value>
        [DefaultValue(null)]
        [DataMember(20)]
        [NotNull]
        public MaterialBlendLayers Layers { get; set; }

        /// <summary>
        /// XXXX
        /// </summary>
        /// <userdoc>
        /// All the color mapping nodes of the materials. They are map descriptions (texture or values) and operations on them.
        /// </userdoc>
        //[DataMember(30)]
        //public Dictionary<string, ComputeNode> Overrides { get; private set; }

        /// <summary>
        /// Gets the parameters.
        /// </summary>
        /// <value>The parameters.</value>
        [DataMember(40)]
        public ParameterCollection Parameters { get; private set; }


        public IEnumerable<AssetReference<MaterialAsset>> FindMaterialReferences()
        {
            foreach (var layer in Layers)
            {
                if (layer.Material != null)
                {
                    yield return layer.Material;
                }
            }
        }


        private class MaterialFactory : IObjectFactory
        {
            public object New(Type type)
            {
                var newMaterial = new MaterialAsset
                {
                    Attributes = ObjectFactory.NewInstance<MaterialAttributes>(),
                    Layers = ObjectFactory.NewInstance<MaterialBlendLayers>(),
                };
                return newMaterial;
            }
        }

        public void Visit(MaterialGeneratorContext context)
        {
            if (Attributes != null)
            {
                Attributes.Visit(context);
            }

            if (Layers != null)
            {
                Layers.Visit(context);
            }
        }
    }
}