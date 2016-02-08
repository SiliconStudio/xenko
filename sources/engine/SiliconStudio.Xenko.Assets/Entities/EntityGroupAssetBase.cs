// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.Linq;
using SiliconStudio.Assets;
using SiliconStudio.Assets.Diff;
using SiliconStudio.Core;

namespace SiliconStudio.Xenko.Assets.Entities
{
    /// <summary>
    /// Base class for entity assets (<see cref="SceneAsset"/> and <see cref="PrefabAsset"/>)
    /// </summary>
    [DataContract()]
    public abstract class EntityGroupAssetBase : AssetComposite
    {
        protected EntityGroupAssetBase()
        {
            Hierarchy = new EntityHierarchyData();
        }

        /// <summary>
        /// Gets or sets the data.
        /// </summary>
        /// <value>
        /// The data.
        /// </value>
        [DataMember(20)]
        public EntityHierarchyData Hierarchy { get; set; }

        public override Asset CreateChildAsset(string location)
        {
            var newAsset = (EntityGroupAssetBase)base.CreateChildAsset(location);

            // CAUTION: We need to re-add entities to the list as we are going to change their ids
            // (and the Hierarchy.Entities list is ordered by Id, so they should not be changed after the entity has been added)
            var newEntities = new List<EntityDesign>(newAsset.Hierarchy.Entities);
            newAsset.Hierarchy.Entities.Clear();

            // Process entities to create new ids for entities and base id
            for (int i = 0; i < Hierarchy.Entities.Count; i++)
            {
                var oldEntityDesign = Hierarchy.Entities[i];
                var newEntityDesign = newEntities[i];
                // Assign a new guid
                newEntityDesign.Entity.Id = Guid.NewGuid();

                // Store the baseid of the new version
                newEntityDesign.Design.BaseId = oldEntityDesign.Entity.Id;
                // Make sure that we don't replicate the base part InstanceId
                newEntityDesign.Design.BasePartInstanceId = null;

                // If entity is root, update RootEntities
                // TODO: might not be optimal if many root entities (should use dictionary and second pass on RootEntities)
                int indexRoot = newAsset.Hierarchy.RootEntities.IndexOf(oldEntityDesign.Entity.Id);
                if (indexRoot >= 0)
                {
                    newAsset.Hierarchy.RootEntities[indexRoot] = newEntityDesign.Entity.Id;
                }

                newAsset.Hierarchy.Entities.Add(newEntityDesign);
            }

            return newAsset;
        }

        /// <summary>
        /// Adds an entity as a part asset.
        /// </summary>
        /// <param name="assetPartBase">The entity asset to be used as a part (must be created directly from <see cref="CreateChildAsset"/>)</param>
        /// <param name="rootEntityId">An optional entity id to attach the part to it. If null, the part will be attached to the root entities of this instance</param>
        public void AddPart(EntityGroupAssetBase assetPartBase, Guid? rootEntityId = null)
        {
            AddPartCore(assetPartBase);

            // If a RootEntityId is given and found in this instance, add them as children of entity
            if (rootEntityId.HasValue && this.Hierarchy.Entities.ContainsKey(rootEntityId.Value))
            {
                var rootEntity = Hierarchy.Entities[rootEntityId.Value];
                foreach (var entityId in assetPartBase.Hierarchy.RootEntities)
                {
                    var entity = assetPartBase.Hierarchy.Entities[entityId];
                    rootEntity.Entity.Transform.Children.Add(entity.Entity.Transform);
                }
            }
            else
            {
                // Else add them as root
                this.Hierarchy.RootEntities.AddRange(assetPartBase.Hierarchy.RootEntities);
            }

            // Add all entities with the correct instance id
            foreach (var entityEntry in assetPartBase.Hierarchy.Entities)
            {
                entityEntry.Design.BasePartInstanceId = assetPartBase.Id;
                this.Hierarchy.Entities.Add(entityEntry);
            }
        }

        public override MergeResult Merge(Asset baseAsset, Asset newBase, List<AssetBase> newBaseParts)
        {
            var entityMerge = new EntityGroupAssetMerge((EntityGroupAssetBase)baseAsset, this, (EntityGroupAssetBase)newBase, newBaseParts);
            return entityMerge.Merge();
        }

        public override IEnumerable<AssetPart> CollectParts()
        {
            foreach (var entityDesign in Hierarchy.Entities)
            {
                yield return new AssetPart(entityDesign.Entity.Id, entityDesign.Design.BaseId, entityDesign.Design.BasePartInstanceId);
            }
        }

        public override bool ContainsPart(Guid id)
        {
            return Hierarchy.Entities.ContainsKey(id);
        }

        /// <summary>
        /// Gets a mapping between a base and the list of instance actually used
        /// </summary>
        /// <param name="baseParts">The list of baseParts to use. If null, use the parts from this instance directly.</param>
        /// <returns>A mapping between a base asset and the list of instance actually used for inherited parts by composition</returns>
        public Dictionary<EntityGroupAssetBase, List<Guid>> GetBasePartInstanceIds(List<AssetBase> baseParts = null)
        {
            if (baseParts == null)
            {
                baseParts = BaseParts;
            }

            var mapBaseToInstanceIds = new Dictionary<EntityGroupAssetBase, List<Guid>>();
            if (baseParts == null)
            {
                return mapBaseToInstanceIds;
            }

            // This method is recovering links between a derived entity from a base part.
            // This is done in 2 steps:


            // Step 1) build the map <mapBasePartInstanceIdToBasePart>: <basePartInstanceId> => <base part asset>  
            //
            // - for each entity in the hierarchy of this instance
            //   - Check if the entity has a <baseId> and a <basePartInstanceId>
            //   - If yes, the entity is coming from a base part
            //       - Find which AssetBase (actually EntityGroupAssetBase), is containing the <basePartInstanceId>
            //       - We can then associate 
            var mapBasePartInstanceIdToBasePart = new Dictionary<Guid, EntityGroupAssetBase>();
            foreach (var entityIt in Hierarchy.Entities)
            {
                if (entityIt.Design.BaseId.HasValue && entityIt.Design.BasePartInstanceId.HasValue)
                {
                    var basePartInstanceId = entityIt.Design.BasePartInstanceId.Value;
                    EntityGroupAssetBase existingAssetBase;
                    if (!mapBasePartInstanceIdToBasePart.TryGetValue(basePartInstanceId, out existingAssetBase))
                    {
                        var baseId = entityIt.Design.BaseId.Value;
                        foreach (var basePart in baseParts)
                        {
                            var assetBase = (EntityGroupAssetBase)basePart.Asset;
                            if (assetBase.ContainsPart(baseId))
                            {
                                existingAssetBase = assetBase;
                                break;
                            }
                        }

                        if (existingAssetBase != null)
                        {
                            mapBasePartInstanceIdToBasePart.Add(basePartInstanceId, existingAssetBase);
                        }
                    }
                }
            }

            // Step 2) build the resulting reverse map <mapBaseToInstanceIds>: <base part asset> => list of <basePartInstanceId>
            //
            // - We simply build this map by using the mapBasePartInstanceIdToBasePart
            foreach (var it in mapBasePartInstanceIdToBasePart)
            {
                List<Guid> ids;
                if (!mapBaseToInstanceIds.TryGetValue(it.Value, out ids))
                {
                    ids = new List<Guid>();
                    mapBaseToInstanceIds.Add(it.Value, ids);
                }
                ids.Add(it.Key);
            }
            return mapBaseToInstanceIds;
        }
    }
}