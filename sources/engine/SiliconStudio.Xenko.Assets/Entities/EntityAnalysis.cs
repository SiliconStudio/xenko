// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.Linq;
using SiliconStudio.Assets.Visitors;
using SiliconStudio.Core;
using SiliconStudio.Core.Reflection;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Engine.Design;

namespace SiliconStudio.Xenko.Assets.Entities
{
    public static class EntityAnalysis
    {
        public struct Result
        {
            public List<EntityLink> EntityReferences;
        }

        public static Result Visit(object entityHierarchy)
        {
            if (entityHierarchy == null) throw new ArgumentNullException("entityHierarchy");

            var entityReferenceVistor = new EntityReferenceAnalysis();
            entityReferenceVistor.Visit(entityHierarchy);

            return entityReferenceVistor.Result;
        }

        private static Result Visit(ComponentBase componentBase)
        {
            if (componentBase == null) throw new ArgumentNullException("componentBase");

            var entityReferenceVistor = new EntityReferenceAnalysis();
            entityReferenceVistor.Visit(componentBase);

            return entityReferenceVistor.Result;
        }

        [Obsolete("This method does not work anymore.")]
        public static void UpdateEntityReferences(EntityHierarchyData entityHierarchy)
        {
            // TODO: Either remove this function or make it do something!
        }


        private static void FixupEntityReferences(object rootToVisit, EntityHierarchyData entityHierarchy)
        {
            var entityAnalysisResult = Visit(rootToVisit);

            // Reverse the list, so that we can still properly update everything
            // (i.e. if we have a[0], a[1], a[1].Test, we have to do it from back to front to be valid at each step)
            entityAnalysisResult.EntityReferences.Reverse();

            // Updates Entity/EntityComponent references
            foreach (var entityLink in entityAnalysisResult.EntityReferences)
            {
                object obj = null;

                if (entityLink.EntityComponent != null)
                {
                    var containingEntity = entityLink.EntityComponent.Entity;
                    if (containingEntity == null)
                    {
                        throw new InvalidOperationException("Found a reference to a component which doesn't have any entity");
                    }

                    EntityDesign realEntity;
                    if (entityHierarchy.Entities.TryGetValue(containingEntity.Id, out realEntity))
                    {
                        var componentId = IdentifiableHelper.GetId(entityLink.EntityComponent);
                        obj = realEntity.Entity.Components.FirstOrDefault(c => IdentifiableHelper.GetId(c) == componentId);
                        if (obj == entityLink.EntityComponent)
                        {
                            continue;
                        }
                    }
                }
                else
                {
                    EntityDesign realEntity;
                    if (entityHierarchy.Entities.TryGetValue(entityLink.Entity.Id, out realEntity))
                    {
                        obj = realEntity.Entity;

                        // If we already have the proper item, let's skip
                        if (obj == entityLink.Entity)
                            continue;
                    }
                }

                if (obj != null)
                {
                    // We could find the referenced item, let's use it
                    entityLink.Path.Apply(rootToVisit, MemberPathAction.ValueSet, obj);
                }
                else
                {
                    // Item could not be found, let's null it
                    entityLink.Path.Apply(rootToVisit, MemberPathAction.ValueClear, null);
                }
            }
        }

        /// <summary>
        /// Fixups the entity references, by clearing invalid <see cref="EntityReference.Id"/>, and updating <see cref="EntityReference.Value"/> (same for components).
        /// </summary>
        /// <param name="entityAssetBase">The entity asset.</param>
        public static void FixupEntityReferences(EntityGroupAssetBase entityAssetBase)
        {
            FixupEntityReferences(entityAssetBase, entityAssetBase.Hierarchy);
        }

        public static void FixupEntityReferences(EntityHierarchyData hierarchyData)
        {
            FixupEntityReferences(hierarchyData, hierarchyData);
        }

        /// <summary>
        /// Remaps the entities identifier.
        /// </summary>
        /// <param name="entityHierarchy">The entity hierarchy.</param>
        /// <param name="idRemapping">The identifier remapping.</param>
        public static void RemapEntitiesId(EntityHierarchyData entityHierarchy, Dictionary<Guid, Guid> idRemapping)
        {
            Guid newId;

            // Remap entities in asset2 with new Id
            for (int i = 0; i < entityHierarchy.RootEntities.Count; ++i)
            {
                if (idRemapping.TryGetValue(entityHierarchy.RootEntities[i], out newId))
                    entityHierarchy.RootEntities[i] = newId;
            }

            foreach (var entity in entityHierarchy.Entities)
            {
                if (idRemapping.TryGetValue(entity.Entity.Id, out newId))
                    entity.Entity.Id = newId;
            }

            // Sort again the EntityCollection (since ID changed)
            entityHierarchy.Entities.Sort();
        }

        private class EntityReferenceAnalysis : AssetVisitorBase
        {
            private int componentDepth;

            private int scriptComponentDepth;

            /// <summary>
            /// The current referencer, should be either an <see cref="Entity"/> or a <see cref="SceneSettings"/>.
            /// </summary>
            private ComponentBase currentReferencer;

            public EntityReferenceAnalysis()
            {
                var result = new Result { EntityReferences = new List<EntityLink>() };
                Result = result;
            }

            public Result Result { get; private set; }

            public override void VisitObject(object obj, ObjectDescriptor descriptor, bool visitMembers)
            {
                bool processObject = true;
                ++scriptComponentDepth;

                if (componentDepth >= 1)
                {
                    var entity = obj as Entity;
                    if (entity != null)
                    {
                        Result.EntityReferences.Add(new EntityLink(currentReferencer, entity, CurrentPath.Clone()));
                        processObject = false;
                    }

                    var entityComponent = obj as EntityComponent;
                    if (entityComponent != null)
                    {
                        Result.EntityReferences.Add(new EntityLink(currentReferencer, entityComponent, CurrentPath.Clone()));
                        processObject = false;
                    }
                }
                else
                {
                    var entity = obj as Entity;
                    if (entity != null)
                        currentReferencer = entity;
                    var settings = obj as SceneSettings;
                    if (settings != null)
                        currentReferencer = settings;
                }

                if (obj is EntityComponent || obj is SceneSettings)
                    componentDepth++;

                if (obj is ScriptComponent)
                    scriptComponentDepth = 0;

                if (processObject)
                    base.VisitObject(obj, descriptor, visitMembers);

                if (obj is EntityComponent || obj is SceneSettings)
                    componentDepth--;

                --scriptComponentDepth;
            }

            protected override bool CanVisit(object obj)
            {
                if (obj is EntityComponent)
                    return true;

                if (obj is SceneSettings)
                    return true;

                return base.CanVisit(obj);
            }
        }

        public struct EntityLink
        {
            public readonly ComponentBase Referencer;
            public readonly Entity Entity;
            public readonly EntityComponent EntityComponent;
            public readonly MemberPath Path;

            public EntityLink(ComponentBase referencer, Entity entity, MemberPath path)
            {
                Referencer = referencer;
                Entity = entity;
                EntityComponent = null;
                Path = path;
            }

            public EntityLink(ComponentBase referencer, EntityComponent entityComponent, MemberPath path)
            {
                Referencer = referencer;
                Entity = null;
                EntityComponent = entityComponent;
                Path = path;
            }
        }
    }
}