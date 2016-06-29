// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Collections.Generic;
using System.ComponentModel;
using SharpYaml.Serialization;
using SiliconStudio.Assets;
using SiliconStudio.Assets.Compiler;
using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Core.IO;
using SiliconStudio.Core.Serialization;
using SiliconStudio.Core.Yaml;
using SiliconStudio.Xenko.Rendering;

namespace SiliconStudio.Xenko.Assets.Model
{
    [DataContract("Model")]
    [AssetDescription(FileExtension, AllowArchetype = false)]
    [AssetCompiler(typeof(ModelAssetCompiler))]
    [Display(190, "Model")]
    [AssetFormatVersion(XenkoConfig.PackageName, "1.5.0-alpha02")]
    [AssetUpgrader(XenkoConfig.PackageName, 0, 2, typeof(Upgrader))]
    [AssetUpgrader(XenkoConfig.PackageName, "0.0.2", "1.4.0-beta", typeof(EmptyAssetUpgrader))]
    [AssetUpgrader(XenkoConfig.PackageName, "1.4.0-beta", "1.5.0-alpha02", typeof(EmptyAssetUpgrader))]
    public sealed class ModelAsset : Asset, IModelAsset, IAssetCompileTimeDependencies
    {
        /// <summary>
        /// The default file extension used by the <see cref="ModelAsset"/>.
        /// </summary>
        public const string FileExtension = ".xkm3d;pdxm3d";

        /// <summary>
        /// Gets or sets the source file of this asset.
        /// </summary>
        /// <value>The source.</value>
        /// <userdoc>
        /// The source file of this asset.
        /// </userdoc>
        [DataMember(-50)]
        [DefaultValue(null)]
        [SourceFileMember(true)]
        public UFile Source { get; set; } = new UFile("");

        /// <summary>
        /// Gets or sets the scale import.
        /// </summary>
        /// <value>The scale import.</value>
        /// <userdoc>The scale applied when importing a model.</userdoc>
        [DataMember(10)]
        [DefaultValue(1.0f)]
        public float ScaleImport { get; set; } = 1.0f;

        /// <inheritdoc/>
        [DataMember(40)]
        [MemberCollection(ReadOnly = true)]
        public List<ModelMaterial> Materials { get; } = new List<ModelMaterial>();

        /// <summary>
        /// Gets or sets the Skeleton.
        /// </summary>
        /// <userdoc>
        /// Describes the node hierarchy that will be active at runtime.
        /// </userdoc>
        [DataMember(50)]
        public Skeleton Skeleton { get; set; }

        [DataMemberIgnore]
        public override UFile MainSource => Source;

        protected override int InternalBuildOrder => -100; // We want Model to be scheduled early since they tend to take the longest (bad concurrency at end of build)

        /// <inheritdoc/>
        public IEnumerable<IReference> EnumerateCompileTimeDependencies()
        {
            if (Skeleton != null)
            {
                var reference = AttachedReferenceManager.GetAttachedReference(Skeleton);
                if (reference != null)
                {
                    yield return new AssetReference<Asset>(reference.Id, reference.Url);
                }
            }
        }

        class Upgrader : AssetUpgraderBase
        {
            protected override void UpgradeAsset(AssetMigrationContext context, PackageVersion currentVersion, PackageVersion targetVersion, dynamic asset, PackageLoadingAssetFile assetFile, OverrideUpgraderHint overrideHint)
            {
                foreach (var modelMaterial in asset.Materials)
                {
                    var material = modelMaterial.Material;
                    if (material != null)
                    {
                        modelMaterial.MaterialInstance = new YamlMappingNode();
                        modelMaterial.MaterialInstance.Material = material;
                        modelMaterial.Material = DynamicYamlEmpty.Default;
                    }
                }
            }
        }
    }
}
