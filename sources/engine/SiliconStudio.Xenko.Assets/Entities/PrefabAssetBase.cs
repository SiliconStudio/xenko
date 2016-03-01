// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using SiliconStudio.Assets;
using SiliconStudio.Assets.Diff;
using SiliconStudio.Core;
using SiliconStudio.Xenko.Engine;

namespace SiliconStudio.Xenko.Assets.Entities
{
    /// <summary>
    /// Base class for entity assets (<see cref="SceneAsset"/> and <see cref="PrefabAsset"/>)
    /// </summary>
    [DataContract()]
    public abstract class PrefabAssetBase : AssetComposite
    {
        protected PrefabAssetBase()
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
            var newAsset = (PrefabAssetBase)base.CreateChildAsset(location);

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
        /// Clones a sub-hierarchy of this asset.
        /// </summary>
        /// <param name="sourceRootEntity">The entity that is the root of the sub-hierarchy to clone</param>
        /// <param name="cleanReference">If true, any reference to an entity external to the cloned hierarchy will be set to null.</param>
        /// <returns></returns>
        public EntityHierarchyData CloneSubHierarchy(Guid sourceRootEntity, bool cleanReference)
        {
            if (!Hierarchy.Entities.ContainsKey(sourceRootEntity))
                throw new ArgumentException(@"The source root entity must be an entity of this asset.", nameof(sourceRootEntity));

            // Note: Instead of copying the whole asset (with its potentially big hierarchy),
            // we first copy the asset only (without the hierarchy), then the sub-hierarchy to extract.
            var subTreeRoot = Hierarchy.Entities[sourceRootEntity].Entity;
            var subTreeHierarchy = new EntityHierarchyData { Entities = { subTreeRoot }, RootEntities = { sourceRootEntity } };
            foreach (var subTreeEntity in EnumerateChildren(subTreeRoot, true))
                subTreeHierarchy.Entities.Add(Hierarchy.Entities[subTreeEntity.Id]);

            // clone the entities of the sub-tree
            var clonedHierarchy = (EntityHierarchyData)AssetCloner.Clone(subTreeHierarchy);
            clonedHierarchy.Entities[sourceRootEntity].Entity.Transform.Parent = null;

            if (cleanReference)
            {
                // set to null reference outside of the sub-tree
                EntityAnalysis.FixupEntityReferences(clonedHierarchy);
            }

            // temporary nullify the hierarchy to avoid to clone it
            var sourceHierarchy = Hierarchy;
            Hierarchy = null;

            // revert the source hierarchy
            Hierarchy = sourceHierarchy;

            // Generate entity mapping
            var reverseEntityMapping = new Dictionary<Guid, Guid>();
            foreach (var entityDesign in clonedHierarchy.Entities)
            {
                // Generate new Id
                var newEntityId = Guid.NewGuid();

                // Update mappings
                reverseEntityMapping.Add(entityDesign.Entity.Id, newEntityId);

                // Update entity with new id
                entityDesign.Entity.Id = newEntityId;
            }

            // Rewrite entity references
            // Should we nullify invalid references?
            EntityAnalysis.RemapEntitiesId(clonedHierarchy, reverseEntityMapping);

            return clonedHierarchy;
        }

        public override MergeResult Merge(Asset baseAsset, Asset newBase, List<AssetBase> newBaseParts)
        {
            var entityMerge = new PrefabAssetMerge((PrefabAssetBase)baseAsset, this, (PrefabAssetBase)newBase, newBaseParts);
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

        public IEnumerable<Entity> EnumerateChildren(Entity entity, bool isRecursive)
        {
            var transformationComponent = entity.Transform;
            if (transformationComponent == null)
                yield break;

            foreach (var child in transformationComponent.Children)
            {
                yield return child.Entity;

                if (isRecursive)
                {
                    foreach (var childChild in EnumerateChildren(child.Entity, true))
                    {
                        yield return childChild;
                    }
                }
            }
        }

        public IEnumerable<EntityDesign> EnumerateChildren(EntityDesign entityDesign, bool isRecursive)
        {
            var transformationComponent = entityDesign.Entity.Transform;
            if (transformationComponent == null)
                yield break;

            foreach (var child in transformationComponent.Children)
            {
                var childEntityDesign = Hierarchy.Entities[child.Entity.Id];
                yield return childEntityDesign;

                if (isRecursive)
                {
                    foreach (var childChild in EnumerateChildren(childEntityDesign, true))
                    {
                        var childChildEntityDesign = Hierarchy.Entities[childChild.Entity.Id];
                        yield return childChildEntityDesign;
                    }
                }
            }
        }


        /// <summary>
        /// Gets a mapping between a base and the list of instance actually used
        /// </summary>
        /// <param name="baseParts">The list of baseParts to use. If null, use the parts from this instance directly.</param>
        /// <returns>A mapping between a base asset and the list of instance actually used for inherited parts by composition</returns>
        public Dictionary<PrefabAssetBase, List<Guid>> GetBasePartInstanceIds(List<AssetBase> baseParts = null)
        {
            if (baseParts == null)
            {
                baseParts = BaseParts;
            }

            var mapBaseToInstanceIds = new Dictionary<PrefabAssetBase, List<Guid>>();
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
            //       - Find which AssetBase (actually PrefabAssetBase), is containing the <basePartInstanceId>
            //       - We can then associate 
            var mapBasePartInstanceIdToBasePart = new Dictionary<Guid, PrefabAssetBase>();
            foreach (var entityIt in Hierarchy.Entities)
            {
                if (entityIt.Design.BaseId.HasValue && entityIt.Design.BasePartInstanceId.HasValue)
                {
                    var basePartInstanceId = entityIt.Design.BasePartInstanceId.Value;
                    PrefabAssetBase existingAssetBase;
                    if (!mapBasePartInstanceIdToBasePart.TryGetValue(basePartInstanceId, out existingAssetBase))
                    {
                        var baseId = entityIt.Design.BaseId.Value;
                        foreach (var basePart in baseParts)
                        {
                            var assetBase = (PrefabAssetBase)basePart.Asset;
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