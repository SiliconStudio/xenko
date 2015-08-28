// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;

using SiliconStudio.Assets.Diff;
using SiliconStudio.Assets.Visitors;
using SiliconStudio.Core.Collections;
using SiliconStudio.Core.Reflection;
using SiliconStudio.Paradox.Engine;
using SiliconStudio.Paradox.Engine.Design;

namespace SiliconStudio.Paradox.Assets.Entities
{
    /// <summary>
    /// Transforms <see cref="EntityHierarchyData"/> nodes into hierarchical <see cref="EntityDiffNode"/>.
    /// </summary>
    [DiffNodeBuilder]
    public class EntityHierarchyDiffNodeBuilder : IDataCustomVisitor
    {
        public bool CanVisit(Type type)
        {
            return (type == typeof(EntityHierarchyData));
        }

        public void Visit(ref VisitorContext context)
        {
            var dataVisitNodeBuilder = (DataVisitNodeBuilder)context.Visitor;

            if (context.Instance is EntityHierarchyData)
            {
                // Create alternative "proxy" object to run diff on
                var entityHierarchy = (EntityHierarchyData)context.Instance;

                var entitiesById = new EntityDictionary(entityHierarchy);
                foreach (var designEntity in entityHierarchy.Entities)
                {
                    entitiesById.Add(designEntity.Entity.Id, designEntity.Entity);
                }

                // Add this object as member, so that it gets processed instead
                dataVisitNodeBuilder.VisitObjectMember(context.Instance, context.Descriptor, new ConvertedDescriptor(context.DescriptorFactory, "EntitiesById", entitiesById), entitiesById);
            }
        }

        class EntityDictionary : TrackingDictionary<Guid, Entity>, IDiffProxy
        {
            private EntityHierarchyData source;

            public EntityDictionary(EntityHierarchyData source)
            {
                this.source = source;
            }

            public void ApplyChanges()
            {
                // "Garbage collect" entities that are not referenced in hierarchy tree anymore
                var entityHashes = new HashSet<Guid>();
                foreach (var rootEntity in source.RootEntities)
                {
                    CollectEntities(entityHashes, rootEntity);
                }

                source.Entities.Clear();
                foreach (var item in this)
                {
                    if (entityHashes.Contains(item.Key))
                        source.Entities.Add(item.Value);
                }

                // Fixup references
                EntityAnalysis.FixupEntityReferences(source);
            }

            private void CollectEntities(HashSet<Guid> entityHashes, Guid rootEntity)
            {
                if (!entityHashes.Add(rootEntity))
                    return;

                EntityDesign designEntity;
                if (!source.Entities.TryGetValue(rootEntity, out designEntity))
                    return;

                var transformationComponent = designEntity.Entity.Get(TransformComponent.Key);

                foreach (var child in transformationComponent.Children)
                {
                    CollectEntities(entityHashes, child.Entity.Id);
                }
            }
        }
    }
}