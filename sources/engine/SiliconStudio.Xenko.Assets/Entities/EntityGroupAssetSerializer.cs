// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using SharpYaml.Serialization;
using SharpYaml.Serialization.Serializers;
using SiliconStudio.Core.Reflection;
using SiliconStudio.Xenko.Engine;
using ITypeDescriptor = SharpYaml.Serialization.ITypeDescriptor;

namespace SiliconStudio.Xenko.Assets.Entities
{
    //[YamlSerializerFactory]
    public class EntityGroupAssetSerializer : ObjectSerializer //, IDataCustomVisitor
    {
        // TODO: Add some comments to explain how this is working and why we need a specialized serializer

        [ThreadStatic]
        private static bool isSerializingAsReference;

        [ThreadStatic]
        private static int sceneSettingsLevel;

        [ThreadStatic]
        private static int componentLevel;

        public override IYamlSerializable TryCreate(SerializerContext context, ITypeDescriptor typeDescriptor)
        {
            if (CanVisit(typeDescriptor.Type))
                return this;

            return null;
        }

        protected override void CreateOrTransformObject(ref ObjectContext objectContext)
        {
            if (isSerializingAsReference)
            {
                // Create appropriate reference type for both serialization and deserialization
                if (objectContext.SerializerContext.IsSerializing)
                {
                    var entityComponent = objectContext.Instance as EntityComponent;
                    if (entityComponent != null)
                    {
                        objectContext.Instance = new EntityComponentReference(entityComponent);
                    }
                    else if (objectContext.Instance is Entity)
                    {
                        objectContext.Instance = new EntityReference { Id = ((Entity)objectContext.Instance).Id };
                    }
                }
                else
                {
                    var type = objectContext.Descriptor.Type;
                    if (typeof(EntityComponent).IsAssignableFrom(type))
                    {
                        objectContext.Instance = new EntityComponentReference() { ComponentType = type };
                    }
                    else if (type == typeof(Entity))
                    {
                        objectContext.Instance = new EntityReference();
                    }
                }
            }

            base.CreateOrTransformObject(ref objectContext);

            // When deserializing, we don't keep the TransformComponent created when the Entity is created
            if (!objectContext.SerializerContext.IsSerializing && objectContext.Instance is Entity)
            {
                var entity = (Entity)objectContext.Instance;
                entity.Components.Clear();
            }
        }

        protected override void TransformObjectAfterRead(ref ObjectContext objectContext)
        {
            if (isSerializingAsReference)
            {
                // Transform the deserialized reference into a fake Entity, EntityComponent, etc...
                // Fake objects will later be fixed later with EntityAnalysis.FixupEntityReferences()
                if (!objectContext.SerializerContext.IsSerializing)
                {
                    var entityComponentReference = objectContext.Instance as EntityComponentReference;
                    if (entityComponentReference != null)
                    {
                        var entityReference = new Entity { Id = entityComponentReference.Entity.Id };
                        var entityComponent = (EntityComponent)Activator.CreateInstance(entityComponentReference.ComponentType);
                        IdentifiableHelper.SetId(entityComponent, entityComponentReference.Id);
                        entityComponent.Entity = entityReference;

                        objectContext.Instance = entityComponent;
                    }
                    else if (objectContext.Instance is EntityReference)
                    {
                        objectContext.Instance = new Entity { Id = ((EntityReference)objectContext.Instance).Id };
                    }
                    else
                    {
                        base.TransformObjectAfterRead(ref objectContext);
                    }
                }
            }
        }

        public override void WriteYaml(ref ObjectContext objectContext)
        {
            var type = objectContext.Descriptor.Type;
            EnterNode(type);

            try
            {
                base.WriteYaml(ref objectContext);
            }
            finally
            {
                LeaveNode(type);
            }
        }

        public override object ReadYaml(ref ObjectContext objectContext)
        {
            var type = objectContext.Descriptor.Type;
            EnterNode(type);

            try
            {
                var result = base.ReadYaml(ref objectContext);

                if (typeof(EntityGroupAssetBase).IsAssignableFrom(type))
                {
                    // Let's fixup entity references after serialization
                    EntityAnalysis.FixupEntityReferences((EntityGroupAssetBase)objectContext.Instance);
                }

                return result;
            }
            finally
            {
                LeaveNode(type);
            }
        }

        private static void EnterNode(Type type)
        {
            // SceneSettings: Pretend we are already inside an entity so add one level
            if (type == typeof(SceneSettings))
            {
                sceneSettingsLevel++;
            }
            else if (typeof(Entity).IsAssignableFrom(type) || typeof(EntityComponent).IsAssignableFrom(type))
            {
                componentLevel++;
            }

            isSerializingAsReference = sceneSettingsLevel > 0 || componentLevel > 2;
        }

        private static void LeaveNode(Type type)
        {
            if (type == typeof(SceneSettings))
            {
                sceneSettingsLevel--;
            }
            else if (typeof(Entity).IsAssignableFrom(type) || typeof(EntityComponent).IsAssignableFrom(type))
            {
                componentLevel--;
            }

            isSerializingAsReference = sceneSettingsLevel > 0 || componentLevel > 2;
        }

        public bool CanVisit(Type type)
        {
            return typeof(EntityGroupAssetBase).IsAssignableFrom(type) || type == typeof(SceneSettings) || typeof(Entity).IsAssignableFrom(type) || typeof(EntityComponent).IsAssignableFrom(type);
        }

        public sealed class TransientEntityComponent : EntityComponent
        {
        }
    }
}