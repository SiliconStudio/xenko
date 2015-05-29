// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using SiliconStudio.Assets;
using SiliconStudio.Assets.Compiler;
using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Core.Reflection;
using SiliconStudio.Core.Serialization;
using SiliconStudio.Paradox.Rendering;
using SiliconStudio.Paradox.Rendering.Materials;
using SiliconStudio.Paradox.Rendering.Materials.ComputeColors;

namespace SiliconStudio.Paradox.Assets.Materials
{
    /// <summary>
    /// The material asset.
    /// </summary>
    [DataContract("MaterialAsset")]
    [AssetDescription(FileExtension)]
    [ThumbnailCompiler(PreviewerCompilerNames.MaterialThumbnailCompilerQualifiedName, true, Priority = -5000)]
    [AssetCompiler(typeof(MaterialAssetCompiler))]
    [ObjectFactory(typeof(MaterialFactory))]
    [Display(115, "Material", "A material")]
    public sealed class MaterialAsset : Asset, IMaterialDescriptor, IAssetCompileTimeDependencies
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
            Attributes = new MaterialAttributes();
            Layers = new MaterialBlendLayers();
            Parameters = new ParameterCollection();
        }

        protected override int InternalBuildOrder
        {
            get { return 100; }
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
        [Category]
        public MaterialBlendLayers Layers { get; set; }

        /// <summary>
        /// Gets or sets the parameters.
        /// </summary>
        /// <value>The parameters.</value>
        [DataMember(30)]
        public ParameterCollection Parameters { get; set; }

        public IEnumerable<AssetReference<MaterialAsset>> FindMaterialReferences()
        {
            foreach (var layer in Layers)
            {
                if (layer.Material != null)
                {
                    var reference = AttachedReferenceManager.GetAttachedReference(layer.Material);
                    yield return new AssetReference<MaterialAsset>(reference.Id, reference.Url);
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
                newMaterial.Attributes.Diffuse = new MaterialDiffuseMapFeature
                {
                    DiffuseMap = new ComputeColor
                    {
                        Value = new Color4(0.98f, 0.9f, 0.7f, 1.0f)
                    }
                };
                newMaterial.Attributes.DiffuseModel = new MaterialDiffuseLambertModelFeature();
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

        /// <inheritdoc/>
        public IEnumerable<IContentReference> EnumerateCompileTimeDependencies()
        {
            foreach (var materialReference in FindMaterialReferences())
            {
                yield return materialReference;
            }
        }
    }
}