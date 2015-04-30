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
                foreach (var entity in entityHierarchy.Entities)
                {
                    entitiesById.Add(entity.Id, entity);
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
                CollectEntities(entityHashes, source.RootEntity);

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

                Entity entity;
                if (!source.Entities.TryGetValue(rootEntity, out entity))
                    return;

                var transformationComponent = entity.Get(TransformComponent.Key);

                foreach (var child in transformationComponent.Children)
                {
                    CollectEntities(entityHashes, child.Entity.Id);
                }
            }
        }
    }
}