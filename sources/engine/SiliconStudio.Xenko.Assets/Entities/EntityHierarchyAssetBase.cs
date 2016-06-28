// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SiliconStudio.Assets;
using SiliconStudio.Assets.Diff;
using SiliconStudio.Assets.Serializers;
using SiliconStudio.Core;
using SiliconStudio.Core.Extensions;
using SiliconStudio.Core.IO;
using SiliconStudio.Core.Reflection;
using SiliconStudio.Xenko.Engine;

namespace SiliconStudio.Xenko.Assets.Entities
{
    /// <summary>
    /// Base class for entity assets (<see cref="SceneAsset"/> and <see cref="PrefabAsset"/>)
    /// </summary>
    [DataContract]
    [AssetPartReference(typeof(Entity), typeof(EntityComponent))]
    [AssetPartReference(typeof(EntityComponent), ReferenceType = typeof(EntityComponentReference))]
    public abstract partial class EntityHierarchyAssetBase : AssetCompositeHierarchy<EntityDesign, Entity>
    {
        /// <summary>
        /// Dumps this asset to a writer for debug purposes.
        /// </summary>
        /// <param name="writer">A text writer output</param>
        /// <param name="name">Name of this asset</param>
        /// <returns><c>true</c> if the dump was sucessful, <c>false</c> otherwise</returns>
        public bool DumpTo(TextWriter writer, string name)
        {
            if (writer == null) throw new ArgumentNullException(nameof(writer));

            writer.WriteLine();
            writer.WriteLine("*************************************");
            writer.WriteLine($"{GetType().Name}: {name}");
            writer.WriteLine("=====================================");
            return Hierarchy?.DumpTo(writer) ?? false;
        }

        public override Asset CreateChildAsset(string location)
        {
            var newAsset = (EntityHierarchyAssetBase)base.CreateChildAsset(location);

            var newIdMaps = Hierarchy.Parts.ToDictionary(x => x.Entity.Id, x => Guid.NewGuid());
            foreach (var entity in newAsset.Hierarchy.Parts)
            {
                // Store the baseid of the new version
                entity.BaseId = entity.Entity.Id;
                // Make sure that we don't replicate the base part InstanceId
                entity.BasePartInstanceId = null;
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
        /// <returns>A <see cref="AssetCompositeHierarchyData{EntityDesign, Entity}"/> corresponding to the cloned entities.</returns>
        public AssetCompositeHierarchyData<EntityDesign, Entity> CloneSubHierarchy(Guid sourceRootEntity, bool cleanReference)
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
        /// <returns>A <see cref="AssetCompositeHierarchyData{EntityDesign, Entity}"/> corresponding to the cloned entities.</returns>
        public AssetCompositeHierarchyData<EntityDesign, Entity> CloneSubHierarchy(Guid sourceRootEntity, bool cleanReference, out Dictionary<Guid, Guid> entityMapping)
        {
            if (!Hierarchy.Parts.ContainsKey(sourceRootEntity))
                throw new ArgumentException(@"The source root entity must be an entity of this asset.", nameof(sourceRootEntity));

            // Note: Instead of copying the whole asset (with its potentially big hierarchy),
            // we first copy the asset only (without the hierarchy), then the sub-hierarchy to extract.
            var subTreeRoot = Hierarchy.Parts[sourceRootEntity];
            var subTreeHierarchy = new AssetCompositeHierarchyData<EntityDesign, Entity> { Parts = { subTreeRoot }, RootPartIds = { sourceRootEntity } };
            foreach (var subTreeEntity in EnumerateChildParts(subTreeRoot, true))
                subTreeHierarchy.Parts.Add(Hierarchy.Parts[subTreeEntity.Entity.Id]);

            // clone the entities of the sub-tree
            var clonedHierarchy = (AssetCompositeHierarchyData<EntityDesign, Entity>)AssetCloner.Clone(subTreeHierarchy);
            clonedHierarchy.Parts[sourceRootEntity].Entity.Transform.Parent = null;

            if (cleanReference)
            {
                // set to null reference outside of the sub-tree
                var tempAsset = new PrefabAsset { Hierarchy = clonedHierarchy };
                tempAsset.FixupPartReferences();
            }

            // temporary nullify the hierarchy to avoid to clone it
            var sourceHierarchy = Hierarchy;
            Hierarchy = null;

            // revert the source hierarchy
            Hierarchy = sourceHierarchy;

            // Generate entity mapping
            entityMapping = new Dictionary<Guid, Guid>();
            foreach (var entityDesign in clonedHierarchy.Parts)
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
        /// <returns>A <see cref="AssetCompositeHierarchyData{EntityDesign, Entity}"/> corresponding to the cloned entities.</returns>
        /// <remarks>The entities passed to this methods must be independent in the hierarchy.</remarks>
        public AssetCompositeHierarchyData<EntityDesign, Entity> CloneSubHierarchies(IEnumerable<Guid> sourceRootEntities, bool cleanReference, out Dictionary<Guid, Guid> entityMapping)
        {
            // Note: Instead of copying the whole asset (with its potentially big hierarchy),
            // we first copy the asset only (without the hierarchy), then the sub-hierarchy to extract.
            var subTreeHierarchy = new AssetCompositeHierarchyData<EntityDesign, Entity>();
            foreach (var sourceRootEntity in sourceRootEntities)
            {
                if (!Hierarchy.Parts.ContainsKey(sourceRootEntity))
                    throw new ArgumentException(@"The source root entities must be entities of this asset.", nameof(sourceRootEntities));

                var subTreeRoot = Hierarchy.Parts[sourceRootEntity].Entity;
                subTreeHierarchy.Parts.Add(new EntityDesign(subTreeRoot));
                subTreeHierarchy.RootPartIds.Add(sourceRootEntity);
                foreach (var subTreeEntity in EnumerateChildParts(subTreeRoot, true))
                    subTreeHierarchy.Parts.Add(Hierarchy.Parts[subTreeEntity.Id]);
            }

            // clone the entities of the sub-tree
            var clonedHierarchy = (AssetCompositeHierarchyData<EntityDesign, Entity>)AssetCloner.Clone(subTreeHierarchy);
            foreach (var rootEntity in clonedHierarchy.RootPartIds)
            {
                clonedHierarchy.Parts[rootEntity].Entity.Transform.Parent = null;
            }

            if (cleanReference)
            {
                // set to null reference outside of the sub-tree
                var tempAsset = new PrefabAsset { Hierarchy = clonedHierarchy };
                tempAsset.FixupPartReferences();
            }

            // temporary nullify the hierarchy to avoid to clone it
            var sourceHierarchy = Hierarchy;
            Hierarchy = null;

            // revert the source hierarchy
            Hierarchy = sourceHierarchy;

            // Generate entity mapping
            entityMapping = new Dictionary<Guid, Guid>();
            foreach (var entityDesign in clonedHierarchy.Parts)
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

        public override MergeResult Merge(Asset baseAsset, Asset newBase, List<AssetBase> newBaseParts, UFile debugLocation = null)
        {
            var entityMerge = new PrefabAssetMerge((EntityHierarchyAssetBase)baseAsset, this, (EntityHierarchyAssetBase)newBase, newBaseParts, debugLocation);
            return entityMerge.Merge();
        }

        public override Entity GetParent(Entity entity)
        {
            return entity.Transform.Parent?.Entity;
        }

        public override IEnumerable<Entity> EnumerateChildParts(Entity entity, bool isRecursive)
        {
            if (entity.Transform == null)
                return Enumerable.Empty<Entity>();

            var enumerator = isRecursive ? entity.Transform.Children.DepthFirst(t => t.Children) : entity.Transform.Children;
            return enumerator.Select(t => t.Entity);
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
            foreach (var entityIt in Hierarchy.Parts)
            {
                if (entityIt.BaseId.HasValue && entityIt.BasePartInstanceId.HasValue)
                {
                    var basePartInstanceId = entityIt.BasePartInstanceId.Value;
                    EntityHierarchyAssetBase existingAssetBase;
                    if (!mapBasePartInstanceIdToBasePart.TryGetValue(basePartInstanceId, out existingAssetBase))
                    {
                        var baseId = entityIt.BaseId.Value;
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

        protected override object ResolveReference(object partReference)
        {
            var entityComponentReference = partReference as EntityComponent;
            if (entityComponentReference != null)
            {
                var containingEntity = entityComponentReference.Entity;
                if (containingEntity == null)
                {
                    throw new InvalidOperationException("Found a reference to a component which doesn't have any entity");
                }

                var realEntity = (Entity)base.ResolveReference(containingEntity);
                if (realEntity == null)
                    return null;

                var componentId = IdentifiableHelper.GetId(entityComponentReference);
                var realComponent = realEntity.Components.FirstOrDefault(c => IdentifiableHelper.GetId(c) == componentId);
                return realComponent;
            }

            return base.ResolveReference(partReference);
        }
    }
}
