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
    /// <summary>
    /// Default serializer for <see cref="PrefabAsset"/> and <see cref="SceneAsset"/>
    /// </summary>
    /// <remarks>
    /// This serializer handle the case where Entity/Components used inside an <see cref="EntityComponent"/> 
    /// or a <see cref="SceneSettings"/>, should be serialized as references instead of by value.
    /// </remarks>
    public class EntityGroupAssetSerializer : ObjectSerializer
    {
        // Entity (level -> 1)
        //   Component (level -> 2)
        //       *Entity/Component as references (level -> 3+) 
        // SceneSettings (level -> 3)
        private const int SerializeComponentAsReferenceLevel = 3;

        /// <summary>
        /// <c>true/c> if the object visited must be serialized as a reference.
        /// </summary>
        private bool IsSerializingAsReference => componentLevel >= SerializeComponentAsReferenceLevel;

        /// <summary>
        /// See <see cref="EnterNode"/> for usage.
        /// </summary>
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
            if (IsSerializingAsReference)
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
            if (IsSerializingAsReference)
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
            if (type == typeof(SceneSettings))
            {
                // Any Entity, Entitycomponent in a SceneSettings are a references
                componentLevel += SerializeComponentAsReferenceLevel;
            }
            else if (typeof(Entity).IsAssignableFrom(type) || typeof(EntityComponent).IsAssignableFrom(type))
            {
                // Everytime we enter into an Entity/EntityComponent, we increase the componentLevel by 1

                // Usually, comes like this:
                // Entity (level -> 1)
                //   Component (level -> 2)
                //       references (level -> 3+)
                componentLevel++;
            }
        }

        private static void LeaveNode(Type type)
        {
            // Restore the level
            if (type == typeof(SceneSettings))
            {
                componentLevel -= SerializeComponentAsReferenceLevel;
            }
            else if (typeof(Entity).IsAssignableFrom(type) || typeof(EntityComponent).IsAssignableFrom(type))
            {
                // Everytime we exit from an Entity/EntityComponent, we decrease the componentLevel by 1
                componentLevel--;
            }
        }

        private static bool CanVisit(Type type)
        {
            return typeof(EntityGroupAssetBase).IsAssignableFrom(type) || type == typeof(SceneSettings) || typeof(Entity).IsAssignableFrom(type) || typeof(EntityComponent).IsAssignableFrom(type);
        }
    }
}