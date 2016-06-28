// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using SiliconStudio.Assets;
using SiliconStudio.Assets.Compiler;
using SiliconStudio.Core;
using SiliconStudio.Xenko.Engine;

namespace SiliconStudio.Xenko.Assets.Entities
{
    [DataContract("PrefabAsset")]
    [AssetDescription(FileExtension, AllowArchetype = false)]
    [AssetFormatVersion(XenkoConfig.PackageName, CurrentVersion)]
    [AssetCompiler(typeof(PrefabAssetCompiler))]
    [AssetUpgrader(XenkoConfig.PackageName, "0.0.0", "1.7.0-beta01", typeof(SpriteComponentUpgrader))]
    [AssetUpgrader(XenkoConfig.PackageName, "1.7.0-beta01", "1.7.0-beta02", typeof(UIComponentRenamingResolutionUpgrader))]
    [AssetUpgrader(XenkoConfig.PackageName, "1.7.0-beta02", "1.7.0-beta03", typeof(ParticleColorAnimationUpgrader))]
    [AssetUpgrader(XenkoConfig.PackageName, "1.7.0-beta03", "1.7.0-beta04", typeof(EntityDesignUpgrader))]
    [Display(195, "Prefab")]
    public class PrefabAsset : EntityHierarchyAssetBase
    {
        private const string CurrentVersion = "1.7.0-beta04";

        /// <summary>
        /// The default file extension used by the <see cref="PrefabAsset"/>.
        /// </summary>
        public const string FileExtension = ".xkprefab";

        /// <summary>
        /// Creates a instance of this prefab that can be added to another <see cref="EntityHierarchyAssetBase"/>.
        /// </summary>
        /// <param name="targetContainer">The container in which the instance will be added.</param>
        /// <param name="targetLocation">The location of the <see paramref="targetContainer"/> asset.</param>
        /// <returns>An <see cref="AssetCompositeHierarchyData{EntityDesign, Entity}"/> containing the cloned entities of </returns>
        /// <remarks>This method will update the <see cref="Asset.BaseParts"/> property of the <see paramref="targetContainer"/>.</remarks>
        public AssetCompositeHierarchyData<EntityDesign, Entity> CreatePrefabInstance(EntityHierarchyAssetBase targetContainer, string targetLocation)
        {
            Guid unused;
            return CreatePrefabInstance(targetContainer, targetLocation, out unused);
        }

        /// <summary>
        /// Creates a instance of this prefab that can be added to another <see cref="EntityHierarchyAssetBase"/>.
        /// </summary>
        /// <param name="targetContainer">The container in which the instance will be added.</param>
        /// <param name="targetLocation">The location of this asset.</param>
        /// <param name="instanceId">The identifier of the created instance.</param>
        /// <returns>An <see cref="AssetCompositeHierarchyData{EntityDesign, Entity}"/> containing the cloned entities of </returns>
        /// <remarks>This method will update the <see cref="Asset.BaseParts"/> property of the <see paramref="targetContainer"/>.</remarks>
        public AssetCompositeHierarchyData<EntityDesign, Entity> CreatePrefabInstance(EntityHierarchyAssetBase targetContainer, string targetLocation, out Guid instanceId)
        {
            var instance = (PrefabAsset)CreateChildAsset(targetLocation);

            targetContainer.AddBasePart(instance.Base);
            instanceId = Guid.NewGuid();
            foreach (var entityEntry in instance.Hierarchy.Parts)
            {
                entityEntry.BasePartInstanceId = instanceId;
            }
            return instance.Hierarchy;
        }
    }
}
