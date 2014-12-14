// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using SharpYaml.Serialization;
using SharpYaml.Serialization.Serializers;
using SiliconStudio.Core.Reflection;
using SiliconStudio.Paradox.Assets.Model.Analysis;
using SiliconStudio.Paradox.EntityModel;
using ITypeDescriptor = SharpYaml.Serialization.ITypeDescriptor;

namespace SiliconStudio.Paradox.Assets.Model
{
    public class EntitySerializer : ObjectSerializer, IDataCustomVisitor
    {
        [ThreadStatic]
        private static int recursionLevel;

        public override IYamlSerializable TryCreate(SerializerContext context, ITypeDescriptor typeDescriptor)
        {
            if (CanVisit(typeDescriptor.Type))
                return this;

            return null;
        }

        protected override void CreateOrTransformObject(ref ObjectContext objectContext)
        {
            // Level 1 is EntityHierarchyData, Level 2 is Entity, Level 3 is EntityComponent
            if (recursionLevel >= 4)
            {
                if (objectContext.SerializerContext.IsSerializing)
                {
                    var entityComponent = objectContext.Instance as EntityComponent;
                    if (entityComponent != null)
                        objectContext.Instance = new EntityComponentReference(entityComponent);
                    else if (objectContext.Instance is Entity)
                        objectContext.Instance = new EntityReference { Id = ((Entity)objectContext.Instance).Id };
                    else
                        throw new InvalidOperationException();
                }
                else
                {
                    var type = objectContext.Descriptor.Type;
                    if (typeof(EntityComponent).IsAssignableFrom(type))
                        objectContext.Instance = new EntityComponentReference();
                    else if (type == typeof(Entity))
                        objectContext.Instance = new EntityReference();
                    else
                        throw new InvalidOperationException();
                }
            }

            base.CreateOrTransformObject(ref objectContext);
        }

        protected override void TransformObjectAfterRead(ref ObjectContext objectContext)
        {
            // Level 1 is EntityHierarchyData, Level 2 is Entity, Level 3 is EntityComponent
            if (recursionLevel >= 4)
            {
                if (!objectContext.SerializerContext.IsSerializing)
                {
                    var entityComponentReference = objectContext.Instance as EntityComponentReference;
                    if (entityComponentReference != null)
                    {
                        var entityReference = new Entity { Id = entityComponentReference.Entity.Id };
                        var entityComponent = (EntityComponent)Activator.CreateInstance(entityComponentReference.ComponentType);
                        entityComponent.Entity = entityReference;

                        objectContext.Instance = entityComponent;
                    }
                    else if (objectContext.Instance is EntityReference)
                        objectContext.Instance = new Entity { Id = ((EntityReference)objectContext.Instance).Id };
                    else
                        throw new InvalidOperationException();
                }
            }
        }

        public override void WriteYaml(ref ObjectContext objectContext)
        {
            // Make sure we start with 0 (in case previous serialization failed with an exception)
            if (objectContext.Descriptor.Type == typeof(EntityHierarchyData))
                recursionLevel = 0;

            recursionLevel++;

            base.WriteYaml(ref objectContext);

            recursionLevel--;
        }

        public override object ReadYaml(ref ObjectContext objectContext)
        {
            // Make sure we start with 0 (in case previous serialization failed with an exception)
            if (objectContext.Descriptor.Type == typeof(EntityHierarchyData))
                recursionLevel = 0;

            recursionLevel++;

            var result = base.ReadYaml(ref objectContext);

            recursionLevel--;

            if (objectContext.Descriptor.Type == typeof(EntityHierarchyData))
            {
                // Let's fixup entity references after serialization
                EntityAnalysis.FixupEntityReferences((EntityHierarchyData)objectContext.Instance);
            }

            return result;
        }

        public bool CanVisit(Type type)
        {
            return type == typeof(EntityHierarchyData) || type == typeof(Entity) || typeof(EntityComponent).IsAssignableFrom(type);
        }

        public void Visit(ref VisitorContext context)
        {
            // Only visit the instance without visiting childrens
            context.Visitor.VisitObject(context.Instance, context.Descriptor, false);
        }
    }
}