// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using SiliconStudio.Assets;
using SiliconStudio.Assets.Compiler;
using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Core.Diagnostics;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Core.Reflection;
using SiliconStudio.Core.Serialization;
using SiliconStudio.Core.Yaml;
using SiliconStudio.Xenko.Rendering;
using SiliconStudio.Xenko.Rendering.Materials;
using SiliconStudio.Xenko.Rendering.Materials.ComputeColors;

namespace SiliconStudio.Xenko.Assets.Materials
{
    /// <summary>
    /// The material asset.
    /// </summary>
    [DataContract("MaterialAsset")]
    [AssetDescription(FileExtension)]
    [AssetCompiler(typeof(MaterialAssetCompiler))]
    [ObjectFactory(typeof(MaterialFactory))]
    [AssetFormatVersion(XenkoConfig.PackageName, "1.4.0-beta")]
    [AssetUpgrader(XenkoConfig.PackageName, 0, 1, typeof(RemoveParametersUpgrader))]
    [AssetUpgrader(XenkoConfig.PackageName, "0.0.1", "1.4.0-beta", typeof(EmptyAssetUpgrader))]
    [Display(115, "Material")]
    public sealed class MaterialAsset : Asset, IMaterialDescriptor, IAssetCompileTimeDependencies
    {
        /// <summary>
        /// The default file extension used by the <see cref="MaterialAsset"/>.
        /// </summary>
        public const string FileExtension = ".xkmat;.pdxmat";

        /// <summary>
        /// Initializes a new instance of the <see cref="MaterialAsset"/> class.
        /// </summary>
        public MaterialAsset()
        {
            Attributes = new MaterialAttributes();
            Layers = new MaterialBlendLayers();
        }

        protected override int InternalBuildOrder
        {
            get { return 100; }
        }

        [DataMemberIgnore]
        public Guid MaterialId
        {
            get
            {
                return Id;
            }
        }

        /// <summary>
        /// Gets or sets the material attributes.
        /// </summary>
        /// <value>The material attributes.</value>
        /// <userdoc>The base attributes of the material.</userdoc>
        [DataMember(10)]
        [NotNull]
        [Display("Attributes", Expand = ExpandRule.Always)]
        public MaterialAttributes Attributes { get; set; }


        /// <summary>
        /// Gets or sets the material compositor.
        /// </summary>
        /// <value>The material compositor.</value>
        /// <userdoc>The layers overriding the base attributes of the material. Layers are displayed from bottom to top.</userdoc>
        [DefaultValue(null)]
        [DataMember(20)]
        [NotNull]
        [Category]
        [MemberCollection(CanReorderItems = true)]
        [NotNullItems]
        public MaterialBlendLayers Layers { get; set; }

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
                    DiffuseMap = new ComputeTextureColor
                    {
                        FallbackValue = new ComputeColor(new Color4(0.98f, 0.9f, 0.7f, 1.0f))
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


        public class RemoveParametersUpgrader : AssetUpgraderBase
        {
            protected override void UpgradeAsset(AssetMigrationContext context, PackageVersion currentVersion, PackageVersion targetVersion, dynamic asset, PackageLoadingAssetFile assetFile)
            {
                asset.Parameters = DynamicYamlEmpty.Default;
            }
        }
    }
}