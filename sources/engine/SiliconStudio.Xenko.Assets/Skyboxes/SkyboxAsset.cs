// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System.Collections.Generic;
using System.ComponentModel;
using SiliconStudio.Assets;
using SiliconStudio.Assets.Compiler;
using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Core.Serialization;
using SiliconStudio.Core.Serialization.Contents;
using SiliconStudio.Core.Yaml;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Rendering.Skyboxes;

namespace SiliconStudio.Xenko.Assets.Skyboxes
{
    /// <summary>
    /// The skybox asset.
    /// </summary>
    [DataContract("SkyboxAsset")]
    [AssetDescription(FileExtension)]
    [AssetContentType(typeof(Skybox))]
    [Display(1000, "Skybox")]
#if SILICONSTUDIO_XENKO_SUPPORT_BETA_UPGRADE
    [AssetFormatVersion(XenkoConfig.PackageName, CurrentVersion)]
    [AssetUpgrader(XenkoConfig.PackageName, "0", "1.11.1.1", typeof(RemoveSkyboxUsage))]
    [AssetUpgrader(XenkoConfig.PackageName, "1.11.1.1", "2.0.0.0", typeof(EmptyAssetUpgrader))]
#else
    [AssetFormatVersion(XenkoConfig.PackageName, CurrentVersion, "2.0.0.0")]
#endif
    public sealed class SkyboxAsset : Asset
    {
        private const string CurrentVersion = "2.0.0.0";

        /// <summary>
        /// The default file extension used by the <see cref="SkyboxAsset"/>.
        /// </summary>
        public const string FileExtension = ".xksky";

        /// <summary>
        /// Initializes a new instance of the <see cref="SkyboxAsset"/> class.
        /// </summary>
        public SkyboxAsset()
        {
            IsSpecularOnly = false;
            DiffuseSHOrder = SkyboxPreFilteringDiffuseOrder.Order3;
            SpecularCubeMapSize = 256;
        }

        /// <summary>
        /// Gets or sets the type of skybox.
        /// </summary>
        /// <value>The type of skybox.</value>
        /// <userdoc>The source to use as skybox</userdoc>
        [DataMember(10)]
        [Display("CubeMap", Expand = ExpandRule.Always)]
        public Texture CubeMap { get; set; }

        /// <summary>
        /// Gets or set if this skybox affects specular only, if <c>false</c> this skybox will affect ambient lighting
        /// </summary>
        /// <userdoc>
        /// Use the skybox only for specular lighting
        /// </userdoc>
        [DataMember(15)]
        [DefaultValue(false)]
        [Display("Specular Only")]
        public bool IsSpecularOnly { get; set; }

        /// <summary>
        /// Gets or sets the diffuse sh order.
        /// </summary>
        /// <value>The diffuse sh order.</value>
        /// <userdoc>Specify the order of the accuracy of spherical harmonics used to calculate the irradiance of the skybox</userdoc>
        [DefaultValue(SkyboxPreFilteringDiffuseOrder.Order3)]
        [Display("Diffuse SH Order")]
        [DataMember(20)]
        public SkyboxPreFilteringDiffuseOrder DiffuseSHOrder { get; set; }

        /// <summary>
        /// Gets or sets the diffuse sh order.
        /// </summary>
        /// <value>The diffuse sh order.</value>
        /// <userdoc>Specify the size of the irradiance cube map used for the specular lighting</userdoc>
        [DefaultValue(256)]
        [Display("Specular CubeMap Size")]
        [DataMember(30)]
        [DataMemberRange(64, int.MaxValue)]
        public int SpecularCubeMapSize { get; set; }

        public IEnumerable<IReference> GetDependencies()
        {
            if (CubeMap != null)
            {
                var reference = AttachedReferenceManager.GetAttachedReference(CubeMap);
                yield return new AssetReference(reference.Id, reference.Url);
            }
        }

        class RemoveSkyboxUsage : AssetUpgraderBase
        {
            protected override void UpgradeAsset(AssetMigrationContext context, PackageVersion currentVersion, PackageVersion targetVersion, dynamic asset, PackageLoadingAssetFile assetFile, OverrideUpgraderHint overrideHint)
            {
                if (asset.Usage != null)
                {
                    if (asset.Usage == "SpecularLighting")
                    {
                        asset.IsSpecularOnly = true;
                    }
                }
                
                asset.Usage = DynamicYamlEmpty.Default;
            }
        }
    }
}
