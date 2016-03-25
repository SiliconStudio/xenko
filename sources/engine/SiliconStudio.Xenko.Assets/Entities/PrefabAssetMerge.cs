// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SiliconStudio.Assets;
using SiliconStudio.Assets.Diff;
using SiliconStudio.Assets.Visitors;
using SiliconStudio.Core.Reflection;
using SiliconStudio.Xenko.Engine;

namespace SiliconStudio.Xenko.Assets.Entities
{
    /// <summary>
    /// This class is responsible for merging and handling asset templating for entities (according to base, new base, and new version of the entity)
    /// </summary>
    internal class PrefabAssetMerge
    {
        private readonly Dictionary<GroupPartKey, BaseEntityEntry> baseEntities;
        private readonly Dictionary<GroupPartKey, BaseEntityEntry> newBaseEntities;
        private readonly Dictionary<GroupPartKey, NewEntityEntry> newEntities; // specific to this instance, GroupPartKey.PartInstanceId is always Guid.Empty
        private readonly HashSet<Guid> entitiesRemovedInNewBase;
        private readonly HashSet<Guid> entitiesToRemoveFromNew;
        private readonly EntityHierarchyAssetBase baseAsset;
        private readonly EntityHierarchyAssetBase newAsset;
        private readonly EntityHierarchyAssetBase newBaseAsset;
        private readonly List<AssetBase> newBaseParts;
        private readonly HashSet<Guid> entitiesInHierarchy;
        private readonly List<Guid> rootEntitiesToAdd;
        private MergeResult result;

        /// <summary>
        /// Initialize a new instance of <see cref="PrefabAssetMerge"/>
        /// </summary>
        /// <param name="baseAsset">The base asset used for merge (can be null).</param>
        /// <param name="newAsset">The new asset (cannot be null)</param>
        /// <param name="newBaseAsset">The new base asset (can be null)</param>
        /// <param name="newBaseParts">The new base parts (can be null)</param>
        public PrefabAssetMerge(EntityHierarchyAssetBase baseAsset, EntityHierarchyAssetBase newAsset, EntityHierarchyAssetBase newBaseAsset, List<AssetBase> newBaseParts)
        {
            if (newAsset == null) throw new ArgumentNullException(nameof(newAsset));

            // We expect to have at least a baseAsset+newBaseAsset or newBaseParts
            if (baseAsset == null && newBaseAsset == null && (newBaseParts == null || newBaseParts.Count == 0)) throw new InvalidOperationException("Cannot merge from base. No bases found");

            this.newAsset = newAsset;
            this.newBaseAsset = newBaseAsset;
            this.newBaseParts = newBaseParts;
            this.baseAsset = baseAsset;
            baseEntities = new Dictionary<GroupPartKey, BaseEntityEntry>();
            newEntities = new Dictionary<GroupPartKey, NewEntityEntry>();
            newBaseEntities = new Dictionary<GroupPartKey, BaseEntityEntry>();
            entitiesRemovedInNewBase = new HashSet<Guid>();
            entitiesToRemoveFromNew = new HashSet<Guid>();
            entitiesInHierarchy = new HashSet<Guid>();
            rootEntitiesToAdd = new List<Guid>();
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

            FixHierarchy();

            CleanupEntities();

            return result;
        }

        /// <summary>
        /// Prepare the merge by computing internal dictionaries
        /// </summary>
        private void PrepareMerge()
        {
            // Process NewAsset
            MapEntities(newAsset.Hierarchy, newEntities);

            // Process BaseAsset
            MapEntities(baseAsset?.Hierarchy, baseEntities);
            if (newAsset.BaseParts != null)
            {
                foreach (var partItem in newAsset.GetBasePartInstanceIds())
                {
                    foreach (var groupPartId in partItem.Value)
                    {
                        // Because we have a groupPartId, it will clone the hierarchy so that
                        // we can safely merge instances later with NewEntity
                        MapEntities(partItem.Key.Hierarchy, baseEntities, groupPartId);
                    }
                }
            }

            // Process NewBaseAsset
            MapEntities(newBaseAsset?.Hierarchy, newBaseEntities);
            if (newBaseParts != null)
            {
                foreach (var partItem in newAsset.GetBasePartInstanceIds(newBaseParts))
                {
                    foreach (var groupPartId in partItem.Value)
                    {
                        // Because we have a groupPartId, it will clone the hierarchy so that
                        // we can safely merge instances later with NewEntity
                        MapEntities(partItem.Key.Hierarchy, newBaseEntities, groupPartId);
                    }
                }
            }

            // Compute Entities Added by newbase (not present in base)
            foreach (var entityFromNewBase in newBaseEntities)
            {
                var entityId = entityFromNewBase.Value.EntityDesign.Entity.Id;
                // For PartInstanceId key, we take it from the entityFromNewBase.Key
                var basePartInstanceId = entityFromNewBase.Key.PartInstanceId;
                var key = new GroupPartKey(basePartInstanceId, entityId);

                if (!baseEntities.ContainsKey(key))
                {
                    var tempHiearchy = new EntityHierarchyData();
                    tempHiearchy.Entities.Add(entityFromNewBase.Value.EntityDesign);

                    // The new entity added by the newbase
                    var newEntityDesign = ((EntityHierarchyData)AssetCloner.Clone(tempHiearchy)).Entities[0];
                    var baseId = newEntityDesign.Entity.Id;
                    var newId = Guid.NewGuid();
                    newEntityDesign.Entity.Id = newId;
                    newEntityDesign.Design.BaseId = baseId;
                    newEntityDesign.Design.BasePartInstanceId = basePartInstanceId;

                    // Because we are going to modify the NewBase we need to clone it
                    // We tag this entry as special, as we will have to process its children differently later in the merge hierarchy
                    var item = new NewEntityEntry(newEntityDesign, null, newEntities.Count)
                    {
                        IsNewBase = true
                    };

                    // Add this to the list of entities from newAsset
                    // Specific to the newEntities, GroupPartKey.PartInstanceId is always Guid.Empty
                    newEntities.Add(new GroupPartKey(null, newId), item);

                    // If the entity is coming from a part and is from root Entities, we need to add it to the rootEntities by default
                    if (basePartInstanceId.HasValue && entityFromNewBase.Value.Hierarchy.RootEntities.Contains(baseId))
                    {
                        rootEntitiesToAdd.Add(newId);
                    }
                }
            }

            // Compute Entities Removed in newbase (present in base)
            foreach (var entityFromBase in baseEntities)
            {
                var entityId = entityFromBase.Value.EntityDesign.Entity.Id;
                var key = new GroupPartKey(entityFromBase.Key.PartInstanceId, entityId);

                if (!newBaseEntities.ContainsKey(key))
                {
                    entitiesRemovedInNewBase.Add(entityId);
                }
            }

            // Compute the GUID -> index in list for RootEntities for Base and NewBase
            var baseRootEntities = new Dictionary<Guid, int>();
            if (baseAsset != null)
            {
                for (int i = 0; i < baseAsset.Hierarchy.RootEntities.Count; i++)
                {
                    var id = baseAsset.Hierarchy.RootEntities[i];
                    baseRootEntities.Add(id, i);
                }
            }
            var newBaseRootEntities = new Dictionary<Guid, int>();
            if (newBaseAsset != null)
            {
                for (int i = 0; i < newBaseAsset.Hierarchy.RootEntities.Count; i++)
                {
                    var id = newBaseAsset.Hierarchy.RootEntities[i];
                    newBaseRootEntities.Add(id, i);
                }
            }

            // Associate all NewEntity entries to their base
            foreach (var newEntityEntry in newEntities.ToList()) // use ToList so we can modify the dictionary while iterating
            {
                // Skip entities that don't have a base, as we don't have to do anything in the merge
                var entityDesign = newEntityEntry.Value.EntityDesign;
                if (!entityDesign.Design.BaseId.HasValue)
                {
                    continue;
                }

                // Else we will associate entries
                var newEntity = entityDesign.Entity;
                var baseId = entityDesign.Design.BaseId.Value;
                var baseKey = new GroupPartKey(entityDesign.Design.BasePartInstanceId, baseId);

                BaseEntityEntry baseRemap;
                BaseEntityEntry newBaseRemap;
                baseEntities.TryGetValue(baseKey, out baseRemap);
                newBaseEntities.TryGetValue(baseKey, out newBaseRemap);

                // Link between base and new entity
                if (baseRemap != null)
                {
                    baseRemap.NewEntity = newEntityEntry.Value;
                }

                if (newBaseRemap != null)
                {
                    newBaseRemap.NewEntity = newEntityEntry.Value;
                }

                // Link the new entity to its base
                newEntityEntry.Value.Base = baseRemap;
                newEntityEntry.Value.NewBase = newBaseRemap;

                if (entitiesRemovedInNewBase.Contains(baseId))
                {
                    entitiesToRemoveFromNew.Add(newEntity.Id);

                    // Else the entity has been removed
                    newEntities.Remove(newEntityEntry.Key);
                }
                else
                {
                    // Remap ids in the RootEntities for base
                    int index;
                    if (baseAsset != null && baseRootEntities.TryGetValue(baseId, out index))
                    {
                        baseRootEntities.Remove(baseId);
                        baseAsset.Hierarchy.RootEntities[index] = newEntity.Id;
                    }

                    // Remap ids in the RootEntities for newBase
                    if (newBaseAsset != null && newBaseRootEntities.TryGetValue(baseId, out index))
                    {
                        newBaseRootEntities.Remove(baseId);
                        newBaseAsset.Hierarchy.RootEntities[index] = newEntity.Id;
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
            foreach (var entityEntry in newEntities.OrderBy(x => x.Value.Order))
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

                    var previousBaseId = baseEntity.Id;
                    var previousNewBaseId = newBaseEntity.Id;

                    // Remap ids to new entity
                    baseEntity.Id = newEntity.Id;
                    newBaseEntity.Id = newEntity.Id;

                    // For entities and components, we will visit only the members of the first level (first entity, or first component) 
                    // but not recursive one (in case a component reference another entity or component)
                    diff.CustomVisitorsBase.Add(new SingleLevelVisitor(typeof(Entity), false));
                    diff.CustomVisitorsBase.Add(new SingleLevelVisitor(typeof(EntityComponent), false));

                    diff.CustomVisitorsAsset1.Add(new SingleLevelVisitor(typeof(Entity), false));
                    diff.CustomVisitorsAsset1.Add(new SingleLevelVisitor(typeof(EntityComponent), false));

                    diff.CustomVisitorsAsset2.Add(new SingleLevelVisitor(typeof(Entity), false));
                    diff.CustomVisitorsAsset2.Add(new SingleLevelVisitor(typeof(EntityComponent), false));

                    // Allow entity to move from base/asset1 to newEntity without any errors
                    newEntity.Components.AllowReplaceForeignEntity = true;
                    // Merge assets
                    var localResult = AssetMerge.Merge(diff, AssetMergePolicies.MergePolicyAsset2AsNewBaseOfAsset1);
                    localResult.CopyTo(result);
                    newEntity.Components.AllowReplaceForeignEntity = false;

                    // Merge folder
                    // If folder was not changed compare to the base, always take the version coming from the new base, otherwise leave the modified version
                    if (baseRemap.EntityDesign.Design.Folder == entityDesign.Design.Folder)
                    {
                        entityDesign.Design.Folder = newBaseRemap.EntityDesign.Design.Folder;
                    }

                    // Restore Ids
                    baseEntity.Id = previousBaseId;
                    newBaseEntity.Id = previousNewBaseId;
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
                var diff = new AssetDiff(baseAsset.Hierarchy.RootEntities, newAsset.Hierarchy.RootEntities, newBaseAsset.Hierarchy.RootEntities)
                {
                    UseOverrideMode = true,
                };
                // Merge collections
                var localResult = AssetMerge.Merge(diff, AssetMergePolicies.MergePolicyAsset2AsNewBaseOfAsset1);
                localResult.CopyTo(result);
            }

            // Add known entities in hierarchy
            var newHierarchy = newAsset.Hierarchy;

            // Add entities coming from parts that are roots
            newHierarchy.RootEntities.AddRange(rootEntitiesToAdd);

            foreach (var rootEntity in newHierarchy.RootEntities)
            {
                entitiesInHierarchy.Add(rootEntity);
            }

            // Process hierarchy level by level
            // This way, we give higher importance to top levels
            var entityIdsToProcess = new List<Guid>(newHierarchy.RootEntities);
            while (entityIdsToProcess.Count > 0)
            {
                entityIdsToProcess = MergeHierarchyByLevel(entityIdsToProcess);
            }
        }

        /// <summary>
        /// This method will remove all entities that are finally not in the hierarchy (potentially removed by previous pass)
        /// </summary>
        private void FixHierarchy()
        { 
            var entitiesNotInHierarchy = newAsset.Hierarchy.Entities.ToDictionary(entityDesign => entityDesign.Entity.Id);
            foreach (var entityId in entitiesInHierarchy)
            {
                entitiesNotInHierarchy.Remove(entityId);
            }

            // Add to the existing list of entities removed and remove from current entities
            foreach (var entityEntry in entitiesNotInHierarchy)
            {
                var entityId = entityEntry.Key;
                entitiesToRemoveFromNew.Add(entityId);

                newAsset.Hierarchy.Entities.Remove(entityId);
            }

            // Collect PartInstanceId,baseId => newId
            // If no group part (plain base), use the empty Guid
            var finalMapBaseIdToNewId = new Dictionary<GroupPartKey, Guid>();
            foreach (var entityEntry in newAsset.Hierarchy.Entities)
            {
                if (entityEntry.Design.BaseId.HasValue)
                {
                    var baseId = entityEntry.Design.BaseId.Value;
                    var groupKey = new GroupPartKey(entityEntry.Design.BasePartInstanceId, baseId);
                    finalMapBaseIdToNewId[groupKey] = entityEntry.Entity.Id;
                }
            }

            // If there were any references to entities that have been removed, we need to clean them
            foreach (var entityEntry in newAsset.Hierarchy.Entities)
            {
                FixReferencesToEntities(entityEntry, finalMapBaseIdToNewId);
            }
        }

        /// <summary>
        /// If an entity was removed from a Children during the FixHierarchy phase, we have to remove it from the asset.
        /// </summary>
        private void CleanupEntities()
        {
            // Collect all Entity ids used (either as RootEntities or child entities)
            var entityIds = new HashSet<Guid>(newAsset.Hierarchy.RootEntities);
            foreach (var entityEntry in newAsset.Hierarchy.Entities)
            {
                foreach (var children in entityEntry.Entity.Transform.Children)
                {
                    entityIds.Add(children.Entity.Id);
                }
            }

            // Remove them from the list
            var tempList = new List<EntityDesign>(newAsset.Hierarchy.Entities);
            foreach (var entityEntry in tempList)
            {
                if (!entityIds.Contains(entityEntry.Entity.Id))
                {
                    newAsset.Hierarchy.Entities.Remove(entityEntry.Entity.Id);
                }
            }
        }

        private void FixReferencesToEntities(EntityDesign newEntityDesign, Dictionary<GroupPartKey, Guid> mapBaseIdToNewId)
        {
            var newEntity = newEntityDesign.Entity;

            // We need to visit all references to entities/components in order to fix references
            // (e.g entities removed, entity added from base referencing an entity from base that we have to redirect to the new child entity...)
            // Suppose for example that:
            // 
            // base   newBase                                      newAsset
            // EA       EA                                           EA'
            // EB       EB                                           EB'
            // EC       EC                                           EC'
            //          ED (+link to EA via script or whather)       ED' + link to EA' (we need to change from EA to EA')
            //
            // So in the example above, when merging ED into newAsset, all references to entities declared in newBase are not automatically 
            // remapped to the new entities in newAsset. This is the purpose of this whole method

            var entityVisitor = new SingleLevelVisitor(typeof(Entity), true);
            var entityComponentVisitor = new SingleLevelVisitor(typeof(EntityComponent), true);
            
            DataVisitNodeBuilder.Run(TypeDescriptorFactory.Default, newEntity, new List<IDataCustomVisitor>()
                {
                    entityVisitor,
                    entityComponentVisitor
                });

            // Fix Entity and EntityComponent references 
            foreach (var idNodes in entityVisitor.References.Concat(entityComponentVisitor.References))
            {
                var id = idNodes.Key;
                var nodes = idNodes.Value;

                // If entity id is not in the current list, it is more likely that it was a link to a base entity
                if (!newAsset.Hierarchy.Entities.ContainsKey(id))
                {
                    var groupKey = new GroupPartKey(newEntityDesign.Design.BasePartInstanceId, id);

                    // We are trying to remap the base id to the new id from known entities from newAsset
                    Guid newId;
                    if (mapBaseIdToNewId.TryGetValue(groupKey, out newId))
                    {
                        var linkedEntity = newAsset.Hierarchy.Entities[newId].Entity;
                        foreach (var node in nodes)
                        {
                            var entityComponent = node.Instance as EntityComponent;
                            if (entityComponent != null)
                            {
                                var entityComponentId = IdentifiableHelper.GetId(entityComponent);
                                // TODO: In case of a DataVisitMember node, we need to set an OverrideType to New if we are actually removing a base value
                                var newEntityComponent = (EntityComponent)linkedEntity.Components.FirstOrDefault(t => IdentifiableHelper.GetId(t) == entityComponentId);
                                node.SetValue(newEntityComponent);
                            }
                            else
                            {
                                // TODO: In case of a DataVisitMember node, we need to set an OverrideType to New if we are actually removing a base value
                                // Else the node applies to an entity
                                node.SetValue(linkedEntity);
                            }
                        }
                    }
                    else
                    {
                        // TODO: In case of a DataVisitMember node, we need to set an OverrideType to New if we are actually removing a base value
                        // If we are trying to link to an entity/component that was removed, we need to remove it
                        foreach (var node in nodes)
                        {
                            node.RemoveValue();
                        }
                    }
                }
            }
        }

        private List<Guid> MergeHierarchyByLevel(List<Guid> entityIds)
        {
            var nextEntityIds = new List<Guid>();
            foreach (var entityId in entityIds)
            {
                var remap = newEntities[new GroupPartKey(null, entityId)];
                var entity = remap.EntityDesign.Entity;

                // If we have a base/newbase, we can 3-ways merge lists
                if (remap.Base != null)
                {
                    // Build a list of Entities Ids for the BaseEntity.Transform.Children remapped to the new entities (using the BasePartInstanceId of the entity being processed)
                    var baseChildrenId = new List<Guid>();

                    var basePartInstanceId = remap.EntityDesign.Design.BasePartInstanceId;

                    if (remap.Base.Children != null)
                    {
                        foreach (var transformComponent in remap.Base.Children)
                        {
                            BaseEntityEntry baseEntry;
                            if (baseEntities.TryGetValue(new GroupPartKey(basePartInstanceId, transformComponent.Entity.Id), out baseEntry) && baseEntry.NewEntity != null)
                            {
                                baseChildrenId.Add(baseEntry.NewEntity.EntityDesign.Entity.Id);
                            }
                        }
                    }

                    // List of Entity ids of the NewEntity.Transform.Children
                    var currentChildrenIds = new List<Guid>();
                    if (remap.Children != null)
                    {
                        foreach (var transformComponent in remap.Children)
                        {
                            currentChildrenIds.Add(transformComponent.Entity.Id);
                        }
                    }

                    // Build a list of Entities Ids for the NewBaseEntity.Transform.Children remapped to the new entities (using the BasePartInstanceId of the entity being processed)
                    var newBaseChildrenIds = new List<Guid>();
                    if (remap.NewBase?.Children != null)
                    {
                        foreach (var transformComponent in remap.NewBase.Children)
                        {
                            BaseEntityEntry baseEntry = null;
                            if (newBaseEntities.TryGetValue(new GroupPartKey(basePartInstanceId, transformComponent.Entity.Id), out baseEntry) && baseEntry.NewEntity != null)
                            {
                                newBaseChildrenIds.Add(baseEntry.NewEntity.EntityDesign.Entity.Id);
                            }
                        }
                    }

                    // Perform a merge only if it is needed
                    if (currentChildrenIds.Count > 0 && (baseChildrenId.Count > 0 || newBaseChildrenIds.Count > 0))
                    {
                        // Perform a merge of a IDs list
                        var diff = new AssetDiff(baseChildrenId, currentChildrenIds, newBaseChildrenIds)
                        {
                            UseOverrideMode = true,
                        };
                        var localResult = AssetMerge.Merge(diff, AssetMergePolicies.MergePolicyAsset2AsNewBaseOfAsset1);
                        localResult.CopyTo(result);
                    }

                    if (remap.Children != null)
                    {
                        remap.Children.Clear();
                    }

                    foreach (var childId in currentChildrenIds)
                    {
                        NewEntityEntry newChildRemap;
                        if (newEntities.TryGetValue(new GroupPartKey(null, childId), out newChildRemap))
                        {
                            if (remap.Children == null)
                            {
                                remap.Children = new List<TransformComponent>();
                            }
                            remap.Children.Add(newChildRemap.EntityDesign.Entity.Transform);
                        }
                    }
                }
                else if (remap.IsNewBase && remap.Children != null)
                {
                    // If the entity is coming from NewBase, we need to transform the previous children
                    // to the new one mapped
                    var children = new List<TransformComponent>(remap.Children);
                    remap.Children.Clear();

                    var basePartInstanceId = remap.EntityDesign.Design.BasePartInstanceId;

                    foreach (var transformComponent in children)
                    {
                        BaseEntityEntry baseToNew = null;
                        if (newBaseEntities.TryGetValue(new GroupPartKey(basePartInstanceId, transformComponent.Entity.Id), out baseToNew) && baseToNew.NewEntity != null)
                        {
                            remap.Children.Add(baseToNew.NewEntity.EntityDesign.Entity.Transform);
                        }
                    }
                    remap.Children = children;
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

        private void MapEntities<T>(EntityHierarchyData hierarchyData, Dictionary<GroupPartKey, T> entities, Guid? instancePartIdArg = null) where T : EntityEntry
        {
            if (hierarchyData == null)
            {
                return;
            }

            // Important, if the hierarchy is coming from a part, we need to clone it entirely
            // and associate correctly the instancePartId with each entities that it is composed of.
            if (instancePartIdArg.HasValue)
            {
                hierarchyData = (EntityHierarchyData)AssetCloner.Clone(hierarchyData);
            }


            foreach (var entityDesign in hierarchyData.Entities)
            {
                if (instancePartIdArg.HasValue)
                {
                    entityDesign.Design.BasePartInstanceId = instancePartIdArg;
                }

                var key = new GroupPartKey(instancePartIdArg, entityDesign.Entity.Id);

                if (entities.ContainsKey(key))
                {
                    continue;
                }

                EntityEntry remap;
                if (typeof(T) == typeof(NewEntityEntry))
                {
                    remap = new NewEntityEntry(entityDesign, hierarchyData, entities.Count);
                }
                else if (typeof(T) == typeof(BaseEntityEntry))
                {
                    remap = new BaseEntityEntry(entityDesign, hierarchyData, entities.Count);
                }
                else
                {
                    throw new ArgumentException($"Invalid type [{typeof(T)}]. Expecting concrete type NewEntityEntry or BaseEntityEntry");
                    
                }

                entities[key] = (T)remap;
            }
        }

        private class BaseEntityEntry : EntityEntry
        {

            public BaseEntityEntry(EntityDesign entityDesign, EntityHierarchyData hierarchy, int order) : base(entityDesign, hierarchy, order)
            {
            }

            /// <summary>
            /// Map a base to the new entity
            /// </summary>
            public NewEntityEntry NewEntity { get; set; }

        }

        private class NewEntityEntry : EntityEntry
        {
            public NewEntityEntry(EntityDesign entityDesign, EntityHierarchyData hierarchy, int order) : base(entityDesign, hierarchy, order)
            {
            }

            /// <summary>
            /// <c>true</c> if this entity is a new entity from the new base
            /// </summary>
            public bool IsNewBase { get; set; }

            /// <summary>
            /// Map this entity to its base
            /// </summary>
            public BaseEntityEntry Base { get; set; }

            /// <summary>
            /// Map this entity to its new base
            /// </summary>
            public BaseEntityEntry NewBase { get; set; }
        }


        [DebuggerDisplay("Remap {EntityDesign}")]
        private abstract class EntityEntry
        {
            public EntityEntry(EntityDesign entityDesign, EntityHierarchyData hierarchy, int order)
            {
                if (entityDesign == null) throw new ArgumentNullException(nameof(entityDesign));
                Order = order;
                Hierarchy = hierarchy;
                EntityDesign = entityDesign;

                // Remove children from transform component in order to process them later
                PushChildren();
            }

            public int Order { get; set; }

            public EntityHierarchyData Hierarchy { get; }

            public readonly EntityDesign EntityDesign;

            public List<TransformComponent> Children;

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

            /// <summary>
            /// Transfers children from entity to this remap instance. Clear the children list on the entity.
            /// </summary>
            private void PushChildren()
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
        }

        /// <summary>
        /// Key used to associate an instance of a base Entity.
        /// The PartInstanceId allows to use the same baseId multiple times if the entity is instantiated multiple times.
        /// </summary>
        [DebuggerDisplay("BaseId: {BaseId} PartInstanceId: {PartInstanceId}")]
        private struct GroupPartKey : IEquatable<GroupPartKey>
        {
            public GroupPartKey(Guid? partInstanceId, Guid baseId)
            {
                PartInstanceId = partInstanceId;
                BaseId = baseId;
            }

            public readonly Guid? PartInstanceId;

            public readonly Guid BaseId;


            public bool Equals(GroupPartKey other)
            {
                return PartInstanceId.Equals(other.PartInstanceId) && BaseId.Equals(other.BaseId);
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                return obj is GroupPartKey && Equals((GroupPartKey)obj);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return (PartInstanceId.GetHashCode()*397) ^ BaseId.GetHashCode();
                }
            }

            public static bool operator ==(GroupPartKey left, GroupPartKey right)
            {
                return left.Equals(right);
            }

            public static bool operator !=(GroupPartKey left, GroupPartKey right)
            {
                return !left.Equals(right);
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