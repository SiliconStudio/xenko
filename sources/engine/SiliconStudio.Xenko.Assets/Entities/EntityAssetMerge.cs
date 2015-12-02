// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.Linq;
using SiliconStudio.Assets;
using SiliconStudio.Assets.Diff;
using SiliconStudio.Assets.Visitors;
using SiliconStudio.Core.Reflection;
using SiliconStudio.Shaders.Utility;
using SiliconStudio.Xenko.Engine;

namespace SiliconStudio.Xenko.Assets.Entities
{
    /// <summary>
    /// This class is responsible for merging entities (according to base, new base, and new version of the entity)
    /// </summary>
    internal class EntityAssetMerge
    {
        private readonly Dictionary<Guid, EntityRemapEntry> baseEntities;
        private readonly Dictionary<Guid, EntityRemapEntry> newBaseEntities;
        private readonly Dictionary<Guid, EntityRemapEntry> newEntities;
        private readonly HashSet<Guid> entitiesAddedByNewBase;
        private readonly HashSet<Guid> entitiesRemovedInNewBase;
        private readonly HashSet<Guid> entitiesToRemoveFromNew;
        private readonly EntityAssetBase baseAsset;
        private readonly EntityAssetBase newAsset;
        private readonly EntityAssetBase newBaseAsset;
        private readonly List<AssetItem> newBaseParts;
        private readonly HashSet<Guid> entitiesInHierarchy;
        private MergeResult result;

        /// <summary>
        /// Initialize a new instance of <see cref="EntityAssetMerge"/>
        /// </summary>
        /// <param name="baseAsset">The base asset used for merge (can be null).</param>
        /// <param name="newAsset">The new asset (cannot be null)</param>
        /// <param name="newBaseAsset">The new base asset (can be null)</param>
        /// <param name="newBaseParts">The new base parts (can be null)</param>
        public EntityAssetMerge(EntityAssetBase baseAsset, EntityAssetBase newAsset, EntityAssetBase newBaseAsset, List<AssetItem> newBaseParts)
        {
            if (newAsset == null) throw new ArgumentNullException(nameof(newAsset));

            // We expect to have at least a baseAsset+newBaseAsset or newBaseParts
            if (baseAsset == null && newBaseAsset == null && (newBaseParts == null || newBaseParts.Count == 0)) throw new InvalidOperationException("Cannot merge from base. No bases found");

            this.newAsset = newAsset;
            this.newBaseAsset = newBaseAsset;
            this.newBaseParts = newBaseParts;
            this.baseAsset = baseAsset;
            baseEntities = new Dictionary<Guid, EntityRemapEntry>();
            newEntities = new Dictionary<Guid, EntityRemapEntry>();
            newBaseEntities = new Dictionary<Guid, EntityRemapEntry>();
            entitiesAddedByNewBase = new HashSet<Guid>();
            entitiesRemovedInNewBase = new HashSet<Guid>();
            entitiesToRemoveFromNew = new HashSet<Guid>();
            entitiesInHierarchy = new HashSet<Guid>();
        }

        /// <summary>
        /// Merges the entities.
        /// </summary>
        /// <returns>The results of the merge.</returns>
        public MergeResult Merge()
        {
            result = new MergeResult(newAsset);

            PrepareMerge();

            MergeEntities();

            MergeHierarchy();

            PurgeEntitiesNotInHierarchy();

            return result;
        }

        /// <summary>
        /// Prepare the merge by computing internal dictionaries
        /// </summary>
        private void PrepareMerge()
        {
            // Prepare mappings for base
            MapEntities(baseAsset?.Hierarchy, baseEntities);

            // Prepare mapping for new asset
            MapEntities(newAsset.Hierarchy, newEntities);
            if (newAsset.BaseParts != null)
            {
                foreach (var partItem in newAsset.BaseParts)
                {
                    var assetPart = (EntityAssetBase)partItem.Asset;
                    MapEntities(assetPart.Hierarchy, baseEntities);
                }
            }

            // Prepare mapping for new base
            MapEntities(newBaseAsset?.Hierarchy, newBaseEntities);
            if (newBaseParts != null)
            {
                foreach (var partItem in newBaseParts)
                {
                    var assetPart = (EntityAssetBase)partItem.Asset;
                    MapEntities(assetPart.Hierarchy, newBaseEntities);
                }
            }

            // Compute Entities Added by newbase (not present in base)
            foreach (var entityFromNewBase in newBaseEntities)
            {
                var entityId = entityFromNewBase.Value.EntityDesign.Entity.Id;
                if (!baseEntities.ContainsKey(entityId) && !entitiesAddedByNewBase.Contains(entityId))
                {
                    entitiesAddedByNewBase.Add(entityId);

                    var item = entityFromNewBase.Value;

                    // The new entity added by the newbase
                    var baseId = item.EntityDesign.Entity.Id;
                    item.EntityDesign.Entity.Id = Guid.NewGuid();
                    item.EntityDesign.Design.BaseId = baseId;

                    // Add this to the list of entities from newAsset
                    newEntities.Add(item.EntityDesign.Entity.Id, item);
                }
            }

            // Compute Entities Removed in newbase (present in base)
            foreach (var entityFromBase in baseEntities)
            {
                var entityId = entityFromBase.Value.EntityDesign.Entity.Id;
                if (newBaseEntities.ContainsKey(entityId))
                {
                    entitiesRemovedInNewBase.Add(entityId);
                }
            }

            // Prebuild the list of entities that we will have to remove
            foreach (var entityEntry in newEntities.ToList()) // use ToList so we can modify the dictionary while iterating
            {
                var entityDesign = entityEntry.Value.EntityDesign;
                var newEntity = entityDesign.Entity;

                if (entityDesign.Design.BaseId.HasValue)
                {
                    var baseId = entityDesign.Design.BaseId.Value;

                    EntityRemapEntry baseRemap;
                    EntityRemapEntry newBaseRemap;
                    baseEntities.TryGetValue(baseId, out baseRemap);
                    newBaseEntities.TryGetValue(baseId, out newBaseRemap);
                    entityEntry.Value.Base = baseRemap;
                    entityEntry.Value.NewBase = newBaseRemap;

                    if (entitiesRemovedInNewBase.Contains(baseId))
                    {
                        entitiesToRemoveFromNew.Add(newEntity.Id);

                        // Else the entity has been removed
                        newEntities.Remove(entityEntry.Key);
                    }
                }
            }
        }

        /// <summary>
        /// This method will merge all entities without taking into account hierarchy that will be handled by <see cref="MergeHierarchy"/>
        /// </summary>
        private void MergeEntities()
        {
            // Clear all entities
            newAsset.Hierarchy.Entities.Clear();

            // Visit all existing entities, coming both from newAsset and new entities from newBase
            foreach (var entityEntry in newEntities)
            {
                var entityDesign = entityEntry.Value.EntityDesign;
                var newEntity = entityDesign.Entity;

                if (entityEntry.Value.Base != null && entityEntry.Value.NewBase != null)
                {
                    var baseRemap = entityEntry.Value.Base;
                    var newBaseRemap = entityEntry.Value.NewBase;
                    var baseEntity = baseRemap.EntityDesign.Entity;
                    var newBaseEntity = newBaseRemap.EntityDesign.Entity;

                    var diff = new AssetDiff(baseEntity, newEntity, newBaseEntity)
                    {
                        UseOverrideMode = true,
                    };

                    // For entities and components, we will visit only the members of the first level (first entity, or first component) 
                    // but not recursive one (in case a component reference another entity or component)
                    diff.CustomVisitorsBase.Add(new SingleLevelVisitor(typeof(Entity), false));
                    diff.CustomVisitorsBase.Add(new SingleLevelVisitor(typeof(EntityComponent), false));

                    diff.CustomVisitorsAsset1.Add(new SingleLevelVisitor(typeof(Entity), false));
                    diff.CustomVisitorsAsset1.Add(new SingleLevelVisitor(typeof(EntityComponent), false));

                    diff.CustomVisitorsAsset2.Add(new SingleLevelVisitor(typeof(Entity), false));
                    diff.CustomVisitorsAsset2.Add(new SingleLevelVisitor(typeof(EntityComponent), false));

                    // Merge assets
                    var localResult = AssetMerge.Merge(diff, AssetMergePolicies.MergePolicyAsset2AsNewBaseOfAsset1);
                    localResult.CopyTo(result);

                    // Merge folder
                    // If folder was not changed compare to the base, always take the version coming from the new base, otherwise leave the modified version
                    if (baseRemap.EntityDesign.Design.Folder == entityDesign.Design.Folder)
                    {
                        entityDesign.Design.Folder = newBaseRemap.EntityDesign.Design.Folder;
                    }
                }

                // Add the entity
                newAsset.Hierarchy.Entities.Add(entityDesign);
            }
        }

        /// <summary>
        /// This method is responsible for merging the hierarchy.
        /// </summary>
        private void MergeHierarchy()
        {
            if (baseAsset != null && newBaseAsset != null)
            {
                // TODO: Merge top-level hiearchy into newAsset
            }

            // Add known entities in hierarchy
            foreach (var rootEntity in newAsset.Hierarchy.RootEntities)
            {
                entitiesInHierarchy.Add(rootEntity);
            }

            // Process hierarchy level by level
            // This way, we give higher importance to top levels
            var newHierarchy = newAsset.Hierarchy;
            var entityIdsToProcess = new List<Guid>(newHierarchy.RootEntities);
            while (entityIdsToProcess.Count > 0)
            {
                entityIdsToProcess = MergeHierarchyByLevel(entityIdsToProcess);
            }
        }

        /// <summary>
        /// This method will remove all entities that are finally not in the hierarchy (potentially removed by previous pass)
        /// </summary>
        private void PurgeEntitiesNotInHierarchy()
        { 
            var allEntities = newAsset.Hierarchy.Entities.ToDictionary(entityDesign => entityDesign.Entity.Id);
            foreach (var entityId in entitiesInHierarchy)
            {
                allEntities.Remove(entityId);
            }

            // Add to the existing list of entities removed and remove from current entities
            foreach (var entityEntry in allEntities)
            {
                var entityId = entityEntry.Key;
                entitiesToRemoveFromNew.Add(entityId);

                newAsset.Hierarchy.Entities.Remove(entityId);
            }

            // If there were any references to entities that have been removed, we need to clean them
            foreach (var entityEntry in newAsset.Hierarchy.Entities)
            {
                CleanupReferencesToEntityRemoved(entityEntry.Entity);
            }
        }

        private void CleanupReferencesToEntityRemoved(Entity newEntity)
        {
            // If we have any entities to remove, we need to go through all references and remove them
            // Remove references to components that were removed
            var entityVisitor = new SingleLevelVisitor(typeof(Entity), true);
            var entityComponentVisitor = new SingleLevelVisitor(typeof(EntityComponent), true);

            DataVisitNodeBuilder.Run(TypeDescriptorFactory.Default, newEntity, new List<IDataCustomVisitor>()
                {
                    entityVisitor,
                    entityComponentVisitor
                });

            foreach (var entityId in entitiesToRemoveFromNew)
            {
                List<DataVisitNode> nodes;
                if (entityVisitor.References.TryGetValue(entityId, out nodes))
                {
                    foreach (var node in nodes)
                    {
                        node.RemoveValue();
                        
                        // TODO: In case of a DataVisitMember node, we need to set an OverrideType to New if we are actually removing a base value
                    }
                }
            }
        }

        private List<Guid> MergeHierarchyByLevel(List<Guid> entityIds)
        {
            var nextEntityIds = new List<Guid>();
            foreach (var entityId in entityIds)
            {
                var remap = newEntities[entityId];
                var entity = remap.EntityDesign.Entity;

                // If we have a base/newbase, we can 3-ways merge lists
                if (remap.Base != null && remap.NewBase != null)
                {
                    var diff = new AssetDiff(remap.Base.Children, remap.Children, remap.NewBase.Children)
                    {
                        UseOverrideMode = true,
                    };

                    // Perform a diff only on the list order but not on the components themselves
                    diff.CustomVisitorsBase.Add(new SingleLevelVisitor(typeof(TransformComponent), false, -1));
                    diff.CustomVisitorsAsset1.Add(new SingleLevelVisitor(typeof(TransformComponent), false, -1));
                    diff.CustomVisitorsAsset2.Add(new SingleLevelVisitor(typeof(TransformComponent), false, -1));

                    // Merge assets
                    var localResult = AssetMerge.Merge(diff, AssetMergePolicies.MergePolicyAsset2AsNewBaseOfAsset1);
                    localResult.CopyTo(result);
                }

                // Popup the children
                remap.PopChildren();

                // For each child, add them to the list of entities in hierarchy and that we will
                // process them in the next round
                for (int i = 0; i < entity.Transform.Children.Count; i++)
                {
                    var transformChild = entity.Transform.Children[i];
                    var subEntityId = transformChild.Entity.Id;
                    if (entitiesInHierarchy.Add(subEntityId))
                    {
                        nextEntityIds.Add(subEntityId);
                    }
                    else
                    {
                        // The entity was already in the hierarchy, so we remove them from this one.
                        entity.Transform.Children.RemoveAt(i);
                        i--;
                    }
                }
            }

            return nextEntityIds;
        }

        private void MapEntities(EntityHierarchyData hierarchyData, Dictionary<Guid, EntityRemapEntry> entities)
        {
            if (hierarchyData == null)
            {
                return;
            }

            foreach (var entity in hierarchyData.Entities)
            {
                if (entities.ContainsKey(entity.Entity.Id))
                {
                    continue;
                }

                var remap = new EntityRemapEntry(entity);
                // We are removing children from transform component for the diff
                remap.PushChildren();
                entities[entity.Entity.Id] = remap;
            }
        }

        private class EntityRemapEntry
        {
            public EntityRemapEntry(EntityDesign entityDesign)
            {
                if (entityDesign == null) throw new ArgumentNullException(nameof(entityDesign));
                EntityDesign = entityDesign;
            }

            public readonly EntityDesign EntityDesign;

            public List<TransformComponent> Children;

            public EntityRemapEntry Base { get; set; }

            public EntityRemapEntry NewBase { get; set; }

            /// <summary>
            /// Transfers children from entity to this remap instance. Clear the children list on the entity.
            /// </summary>
            public void PushChildren()
            {
                foreach (var child in EntityDesign.Entity.Transform.Children)
                {
                    if (Children == null)
                    {
                        Children = new List<TransformComponent>();
                    }
                    Children.Add(child);
                }
                EntityDesign.Entity.Transform.Children.Clear();
            }

            /// <summary>
            /// Transfrers the children from this remap instance to the associated entity. Clear the children list of this instance.
            /// </summary>
            public void PopChildren()
            {
                if (Children != null)
                {
                    EntityDesign.Entity.Transform.Children.AddRange(Children);
                    Children.Clear();
                }
            }
        }

        private class SingleLevelVisitor : IDataCustomVisitor
        {
            private readonly Type rootType;
            private readonly bool recordReferences;
            private readonly int lastLevelVisit;
            private int level;

            public SingleLevelVisitor(Type type, bool recordReferences, int lastLevelVisit = 0)
            {
                rootType = type;
                this.recordReferences = recordReferences;
                this.lastLevelVisit = lastLevelVisit;
                References = new Dictionary<Guid, List<DataVisitNode>>();
            }

            public bool CanVisit(Type type)
            {
                return rootType.IsAssignableFrom(type);
            }

            public Dictionary<Guid, List<DataVisitNode>> References { get; }

            public void Reset()
            {
                level = 0;
                References.Clear();
            }

            public void Visit(ref VisitorContext context)
            {
                // Record reference of the instance being visited
                var nodeBuilder = (DataVisitNodeBuilder)context.Visitor;

                if (level == lastLevelVisit)
                {
                    level++;
                    context.Visitor.VisitObject(context.Instance, context.Descriptor, true);
                    level--;
                }
                else if (recordReferences)
                {
                    // Save Entity reference
                    var entity = context.Instance as Entity;
                    var entityComponent = context.Instance as EntityComponent;
                    Guid? id = entity?.Id ?? entityComponent?.Entity.Id;
                    if (id.HasValue)
                    {
                        List<DataVisitNode> nodes;
                        if (!References.TryGetValue(id.Value, out nodes))
                        {
                            nodes = new List<DataVisitNode>();
                            References.Add(id.Value, nodes);
                        }
                        nodes.Add(nodeBuilder.CurrentNode);
                    }
                }
            }
        }
    }
}