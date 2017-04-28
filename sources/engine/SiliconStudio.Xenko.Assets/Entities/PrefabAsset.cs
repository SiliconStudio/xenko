// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using SiliconStudio.Assets;
using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Xenko.Engine;

namespace SiliconStudio.Xenko.Assets.Entities
{
    [DataContract("PrefabAsset")]
    [AssetDescription(FileExtension, AllowArchetype = false)]
    [AssetContentType(typeof(Prefab))]
#if SILICONSTUDIO_XENKO_SUPPORT_BETA_UPGRADE
    [AssetFormatVersion(XenkoConfig.PackageName, CurrentVersion, "1.9.0-beta05")]
    [AssetUpgrader(XenkoConfig.PackageName, "1.9.0-beta05", "1.10.0-beta01", typeof(MoveRenderGroupInsideComponentUpgrader))]
    [AssetUpgrader(XenkoConfig.PackageName, "1.10.0-beta01", "1.10.0-beta02", typeof(FixPartReferenceUpgrader))]
    [AssetUpgrader(XenkoConfig.PackageName, "1.10.0-beta02", "2.0.0.0", typeof(EmptyAssetUpgrader))]
#else
    [AssetFormatVersion(XenkoConfig.PackageName, CurrentVersion, "2.0.0.0")]
#endif
    [AssetUpgrader(XenkoConfig.PackageName, "2.0.0.0", "2.1.0.1", typeof(RootPartIdsToRootPartsUpgrader))]    
    [Display(1950, "Prefab")]
    public class PrefabAsset : EntityHierarchyAssetBase
    {
        private const string CurrentVersion = "2.1.0.1";

        /// <summary>
        /// The default file extension used by the <see cref="PrefabAsset"/>.
        /// </summary>
        public const string FileExtension = ".xkprefab";

        /// <summary>
        /// Creates a instance of this prefab that can be added to another <see cref="EntityHierarchyAssetBase"/>.
        /// </summary>
        /// <param name="targetLocation">The location of the target container asset.</param>
        /// <returns>An <see cref="AssetCompositeHierarchyData{EntityDesign, Entity}"/> containing the cloned entities of </returns>
        [NotNull]
        public AssetCompositeHierarchyData<EntityDesign, Entity> CreatePrefabInstance([NotNull] string targetLocation)
        {
            Guid unused;
            return CreatePrefabInstance(targetLocation, out unused);
        }

        /// <summary>
        /// Creates a instance of this prefab that can be added to another <see cref="EntityHierarchyAssetBase"/>.
        /// </summary>
        /// <param name="targetLocation">The location of the target container asset.</param>
        /// <param name="instanceId">The identifier of the created instance.</param>
        /// <returns>An <see cref="AssetCompositeHierarchyData{EntityDesign, Entity}"/> containing the cloned entities of </returns>
        [NotNull]
        public AssetCompositeHierarchyData<EntityDesign, Entity> CreatePrefabInstance([NotNull] string targetLocation, out Guid instanceId)
        {
            Dictionary<Guid, Guid> idRemapping;
            var instance = (PrefabAsset)CreateDerivedAsset(targetLocation, out idRemapping);
            instanceId = instance.Hierarchy.Parts.FirstOrDefault()?.Base?.InstanceId ?? Guid.NewGuid();
            return instance.Hierarchy;
        }
    }
}
