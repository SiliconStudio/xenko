// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Collections.Generic;
using System.ComponentModel;
using SiliconStudio.Assets;
using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Core.Serialization;
using SiliconStudio.Xenko.Rendering;
using SiliconStudio.Xenko.Rendering.Materials;

namespace SiliconStudio.Xenko.Assets.Materials
{
    /// <summary>
    /// The material asset.
    /// </summary>
    [DataContract("MaterialAsset")]
    [AssetDescription(FileExtension)]
    [AssetContentType(typeof(Material))]
    [Display(1150, "Material")]
#if SILICONSTUDIO_XENKO_SUPPORT_BETA_UPGRADE
    [AssetFormatVersion(XenkoConfig.PackageName, CurrentVersion, "1.4.0-beta")]
    [AssetUpgrader(XenkoConfig.PackageName, "1.4.0-beta", "2.0.0.0", typeof(EmptyAssetUpgrader))]
#else
    [AssetFormatVersion(XenkoConfig.PackageName, CurrentVersion, "2.0.0.0")]
#endif
    public sealed class MaterialAsset : Asset, IMaterialDescriptor
    {
        private const string CurrentVersion = "2.0.0.0";

        /// <summary>
        /// The default file extension used by the <see cref="MaterialAsset"/>.
        /// </summary>
        public const string FileExtension = ".xkmat";

        /// <summary>
        /// Initializes a new instance of the <see cref="MaterialAsset"/> class.
        /// </summary>
        public MaterialAsset()
        {
            Attributes = new MaterialAttributes();
            Layers = new MaterialBlendLayers();
        }

        [DataMemberIgnore]
        public AssetId MaterialId => Id;

        /// <summary>
        /// Gets or sets the material attributes.
        /// </summary>
        /// <value>The material attributes.</value>
        /// <userdoc>The base attributes of the material.</userdoc>
        [DataMember(10)]
        [Display("Attributes", Expand = ExpandRule.Always)]
        public MaterialAttributes Attributes { get; set; }


        /// <summary>
        /// Gets or sets the material compositor.
        /// </summary>
        /// <value>The material compositor.</value>
        /// <userdoc>The layers overriding the base attributes of the material. Layers are displayed from bottom to top.</userdoc>
        [DefaultValue(null)]
        [DataMember(20)]
        [Category]
        [MemberCollection(CanReorderItems = true, NotNullItems = true)]
        public MaterialBlendLayers Layers { get; set; }

        public IEnumerable<AssetReference> FindMaterialReferences()
        {
            foreach (var layer in Layers)
            {
                if (layer.Material != null)
                {
                    var reference = AttachedReferenceManager.GetAttachedReference(layer.Material);
                    yield return new AssetReference(reference.Id, reference.Url);
                }
            }
        }

        public void Visit(MaterialGeneratorContext context)
        {
            Attributes.Visit(context);
            Layers.Visit(context);
        }
    }
}
