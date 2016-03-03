// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using SiliconStudio.Assets;
using SiliconStudio.Assets.Compiler;
using SiliconStudio.Core;

namespace SiliconStudio.Xenko.Assets.Entities
{
    [DataContract("PrefabAsset")]
    [AssetDescription(FileExtension)]
    [AssetCompiler(typeof(PrefabAssetCompiler))]
    [Display(195, "Prefab")]
    public class PrefabAsset : PrefabAssetBase
    {
        /// <summary>
        /// The default file extension used by the <see cref="PrefabAsset"/>.
        /// </summary>
        public const string FileExtension = ".xkprefab";

        /// <summary>
        /// Creates a instance of this prefab that can be added to another <see cref="PrefabAssetBase"/>.
        /// </summary>
        /// <param name="targetContainer">The container in which the instance will be added.</param>
        /// <param name="targetLocation">The location of the <see paramref="targetContainer"/> asset.</param>
        /// <returns>An <see cref="EntityHierarchyData"/> containing the cloned entities of </returns>
        /// <remarks>This method will update the <see cref="Asset.BaseParts"/> property of the <see paramref="targetContainer"/>.</remarks>
        public EntityHierarchyData CreatePrefabInstance(PrefabAssetBase targetContainer, string targetLocation)
        {
            Guid unused;
            return CreatePrefabInstance(targetContainer, targetLocation, out unused);
        }

        /// <summary>
        /// Creates a instance of this prefab that can be added to another <see cref="PrefabAssetBase"/>.
        /// </summary>
        /// <param name="targetContainer">The container in which the instance will be added.</param>
        /// <param name="targetLocation">The location of this asset.</param>
        /// <param name="instanceId">The identifier of the created instance.</param>
        /// <returns>An <see cref="EntityHierarchyData"/> containing the cloned entities of </returns>
        /// <remarks>This method will update the <see cref="Asset.BaseParts"/> property of the <see paramref="targetContainer"/>.</remarks>
        public EntityHierarchyData CreatePrefabInstance(PrefabAssetBase targetContainer, string targetLocation, out Guid instanceId)
        {
            var instance = (PrefabAsset)CreateChildAsset(targetLocation);

            targetContainer.AddBasePart(instance.Base);
            instanceId = Guid.NewGuid();
            foreach (var entityEntry in instance.Hierarchy.Entities)
            {
                entityEntry.Design.BasePartInstanceId = instanceId;
            }
            return instance.Hierarchy;
        }
    }
}