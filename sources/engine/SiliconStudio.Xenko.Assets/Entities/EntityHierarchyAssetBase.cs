// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.Linq;
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
    public abstract class EntityHierarchyAssetBase : AssetComposite
    {
        protected EntityHierarchyAssetBase()
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
            var newAsset = (EntityHierarchyAssetBase)base.CreateChildAsset(location);

            var newIdMaps = Hierarchy.Entities.ToDictionary(x => x.Entity.Id, x => Guid.NewGuid());
            foreach (var entity in newAsset.Hierarchy.Entities)
            {
                // Store the baseid of the new version
                entity.Design.BaseId = entity.Entity.Id;
                // Make sure that we don't replicate the base part InstanceId
                entity.Design.BasePartInstanceId = null;
                // Apply the new Guid
                entity.Entity.Id = newIdMaps[entity.Entity.Id];
            }

            EntityAnalysis.RemapEntitiesId(newAsset.Hierarchy, newIdMaps);

            return newAsset;
        }

        /// <summary>
        /// Clones a sub-hierarchy of this asset.
        /// </summary>
        /// <param name="sourceRootEntity">The entity that is the root of the sub-hierarchy to clone</param>
        /// <param name="cleanReference">If true, any reference to an entity external to the cloned hierarchy will be set to null.</param>
        /// <returns>A <see cref="EntityHierarchyData"/> corresponding to the cloned entities.</returns>
        public EntityHierarchyData CloneSubHierarchy(Guid sourceRootEntity, bool cleanReference)
        {
            Dictionary<Guid, Guid> entityMapping;
            return CloneSubHierarchy(sourceRootEntity, cleanReference, out entityMapping);
        }

        /// <summary>
        /// Clones a sub-hierarchy of this asset.
        /// </summary>
        /// <param name="sourceRootEntity">The entity that is the root of the sub-hierarchy to clone</param>
        /// <param name="cleanReference">If true, any reference to an entity external to the cloned hierarchy will be set to null.</param>
        /// <param name="entityMapping">A dictionary containing the mapping of ids from the source entites to the new entities.</param>
        /// <returns>A <see cref="EntityHierarchyData"/> corresponding to the cloned entities.</returns>
        public EntityHierarchyData CloneSubHierarchy(Guid sourceRootEntity, bool cleanReference, out Dictionary<Guid, Guid> entityMapping)
        {
            if (!Hierarchy.Entities.ContainsKey(sourceRootEntity))
                throw new ArgumentException(@"The source root entity must be an entity of this asset.", nameof(sourceRootEntity));

            // Note: Instead of copying the whole asset (with its potentially big hierarchy),
            // we first copy the asset only (without the hierarchy), then the sub-hierarchy to extract.
            var subTreeRoot = Hierarchy.Entities[sourceRootEntity];
            var subTreeHierarchy = new EntityHierarchyData { Entities = { subTreeRoot }, RootEntities = { sourceRootEntity } };
            foreach (var subTreeEntity in EnumerateChildren(subTreeRoot, true))
                subTreeHierarchy.Entities.Add(Hierarchy.Entities[subTreeEntity.Entity.Id]);

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
            entityMapping = new Dictionary<Guid, Guid>();
            foreach (var entityDesign in clonedHierarchy.Entities)
            {
                // Generate new Id
                var newEntityId = Guid.NewGuid();

                // Update mappings
                entityMapping.Add(entityDesign.Entity.Id, newEntityId);

                // Update entity with new id
                entityDesign.Entity.Id = newEntityId;
            }

            // Rewrite entity references
            // Should we nullify invalid references?
            EntityAnalysis.RemapEntitiesId(clonedHierarchy, entityMapping);

            return clonedHierarchy;
        }

        /// <summary>
        /// Clones a sub-hierarchy of this asset.
        /// </summary>
        /// <param name="sourceRootEntities">The entities that are the roots of the sub-hierarchies to clone</param>
        /// <param name="cleanReference">If true, any reference to an entity external to the cloned hierarchy will be set to null.</param>
        /// <param name="entityMapping">A dictionary containing the mapping of ids from the source entites to the new entities.</param>
        /// <returns>A <see cref="EntityHierarchyData"/> corresponding to the cloned entities.</returns>
        /// <remarks>The entities passed to this methods must be independent in the hierarchy.</remarks>
        public EntityHierarchyData CloneSubHierarchies(IEnumerable<Guid> sourceRootEntities, bool cleanReference, out Dictionary<Guid, Guid> entityMapping)
        {
            // Note: Instead of copying the whole asset (with its potentially big hierarchy),
            // we first copy the asset only (without the hierarchy), then the sub-hierarchy to extract.
            var subTreeHierarchy = new EntityHierarchyData();
            foreach (var sourceRootEntity in sourceRootEntities)
            {
                if (!Hierarchy.Entities.ContainsKey(sourceRootEntity))
                    throw new ArgumentException(@"The source root entities must be entities of this asset.", nameof(sourceRootEntities));

                var subTreeRoot = Hierarchy.Entities[sourceRootEntity].Entity;
                subTreeHierarchy.Entities.Add(subTreeRoot);
                subTreeHierarchy.RootEntities.Add(sourceRootEntity);
                foreach (var subTreeEntity in EnumerateChildren(subTreeRoot, true))
                    subTreeHierarchy.Entities.Add(Hierarchy.Entities[subTreeEntity.Id]);
            }

            // clone the entities of the sub-tree
            var clonedHierarchy = (EntityHierarchyData)AssetCloner.Clone(subTreeHierarchy);
            foreach (var rootEntity in clonedHierarchy.RootEntities)
            {
                clonedHierarchy.Entities[rootEntity].Entity.Transform.Parent = null;
            }

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
            entityMapping = new Dictionary<Guid, Guid>();
            foreach (var entityDesign in clonedHierarchy.Entities)
            {
                // Generate new Id
                var newEntityId = Guid.NewGuid();

                // Update mappings
                entityMapping.Add(entityDesign.Entity.Id, newEntityId);

                // Update entity with new id
                entityDesign.Entity.Id = newEntityId;
            }

            // Rewrite entity references
            // Should we nullify invalid references?
            EntityAnalysis.RemapEntitiesId(clonedHierarchy, entityMapping);

            return clonedHierarchy;
        }

        public override MergeResult Merge(Asset baseAsset, Asset newBase, List<AssetBase> newBaseParts)
        {
            var entityMerge = new PrefabAssetMerge((EntityHierarchyAssetBase)baseAsset, this, (EntityHierarchyAssetBase)newBase, newBaseParts);
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
        public Dictionary<EntityHierarchyAssetBase, List<Guid>> GetBasePartInstanceIds(List<AssetBase> baseParts = null)
        {
            if (baseParts == null)
            {
                baseParts = BaseParts;
            }

            var mapBaseToInstanceIds = new Dictionary<EntityHierarchyAssetBase, List<Guid>>();
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
            //       - Find which AssetBase (actually EntityHierarchyAssetBase), is containing the <basePartInstanceId>
            //       - We can then associate 
            var mapBasePartInstanceIdToBasePart = new Dictionary<Guid, EntityHierarchyAssetBase>();
            foreach (var entityIt in Hierarchy.Entities)
            {
                if (entityIt.Design.BaseId.HasValue && entityIt.Design.BasePartInstanceId.HasValue)
                {
                    var basePartInstanceId = entityIt.Design.BasePartInstanceId.Value;
                    EntityHierarchyAssetBase existingAssetBase;
                    if (!mapBasePartInstanceIdToBasePart.TryGetValue(basePartInstanceId, out existingAssetBase))
                    {
                        var baseId = entityIt.Design.BaseId.Value;
                        foreach (var basePart in baseParts)
                        {
                            var assetBase = (EntityHierarchyAssetBase)basePart.Asset;
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