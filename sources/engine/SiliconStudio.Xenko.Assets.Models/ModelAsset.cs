// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System.Collections.Generic;
using System.ComponentModel;
using SiliconStudio.Assets;
using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Core.IO;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Core.Yaml;
using SiliconStudio.Core.Yaml.Serialization;
using SiliconStudio.Xenko.Rendering;

namespace SiliconStudio.Xenko.Assets.Models
{
    [DataContract("Model")]
    [AssetDescription(FileExtension, AllowArchetype = true)]
    [AssetContentType(typeof(Model))]
    [Display(1900, "Model")]
#if SILICONSTUDIO_XENKO_SUPPORT_BETA_UPGRADE
    [AssetFormatVersion(XenkoConfig.PackageName, CurrentVersion, "1.5.0-alpha02")]
    [AssetUpgrader(XenkoConfig.PackageName, "1.5.0-alpha02", "2.0.0.0", typeof(EmptyAssetUpgrader))]
#else
    [AssetFormatVersion(XenkoConfig.PackageName, CurrentVersion, "2.0.0.0")]
#endif
    public sealed class ModelAsset : Asset, IModelAsset
    {
        private const string CurrentVersion = "2.0.0.0";

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
        /// Gets or sets the pivot position, that will be used as center of object. If a Skeleton is set, its value will be used instead.
        /// </summary>
        /// <userdoc>
        /// The root (pivot) of the animation will be offset by this distance. If a Skeleton is set, its value will be used instead.
        /// </userdoc>
        [DataMember(10)]
        public Vector3 PivotPosition { get; set; }

        /// <summary>
        /// Gets or sets the scale import. If a Skeleton is set, its value will be used instead.
        /// </summary>
        /// <userdoc>The scale applied when importing a model. If a Skeleton is set, its value will be used instead.</userdoc>
        [DataMember(15)]
        [DefaultValue(1.0f)]
        public float ScaleImport { get; set; } = 1.0f;

        /// <inheritdoc/>
        [DataMember(40)]
        [MemberCollection(ReadOnly = true)]
        [Category]
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
