// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;

using SharpYaml.Serialization;
using SharpYaml.Serialization.Serializers;

using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Engine.Design;

namespace SiliconStudio.Xenko.Assets.Entities
{
    //[YamlSerializerFactory]
    public class EntitySerializer : ObjectSerializer //, IDataCustomVisitor
    {
        [ThreadStatic]
        private static int recursionLevel;

        [ThreadStatic]
        private static int levelSinceScriptComponent;

        private static int recursionMaxExpectedDepth;

        public override IYamlSerializable TryCreate(SerializerContext context, ITypeDescriptor typeDescriptor)
        {
            if (CanVisit(typeDescriptor.Type))
                return this;

            return null;
        }

        protected override void CreateOrTransformObject(ref ObjectContext objectContext)
        {
            if (recursionLevel >= recursionMaxExpectedDepth)
            {
                // Create appropriate reference type for both serialization and deserialization
                if (objectContext.SerializerContext.IsSerializing)
                {
                    var entityComponent = objectContext.Instance as EntityComponent;
                    var entityScript = objectContext.Instance as Script;
                    if (entityComponent != null)
                    {
                        objectContext.Instance = new EntityComponentReference(entityComponent);
                    }
                    else if (entityScript != null && levelSinceScriptComponent != 1)
                    {
                        var script = new EntityScriptReference(entityScript);
                        objectContext.Instance = script;
                        objectContext.Tag = objectContext.Settings.TagTypeRegistry.TagFromType(entityScript.GetType());
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
                        objectContext.Instance = new EntityComponentReference();
                    }
                    else if (typeof(Script).IsAssignableFrom(type) && levelSinceScriptComponent != 1)
                    {
                        objectContext.Instance = new EntityScriptReference { ScriptType = objectContext.Descriptor.Type };
                    }
                    else if (type == typeof(Entity))
                    {
                        objectContext.Instance = new EntityReference();
                    }
                }
            }

            base.CreateOrTransformObject(ref objectContext);
        }

        protected override void TransformObjectAfterRead(ref ObjectContext objectContext)
        {
            if (recursionLevel >= recursionMaxExpectedDepth)
            {
                // Transform the deserialized reference into a fake Entity, EntityComponent, etc...
                // Fake objects will later be fixed later with EntityAnalysis.FixupEntityReferences()
                if (!objectContext.SerializerContext.IsSerializing)
                {
                    var entityComponentReference = objectContext.Instance as EntityComponentReference;
                    var entityScriptReference = objectContext.Instance as EntityScriptReference;
                    if (entityComponentReference != null)
                    {
                        var entityReference = new Entity { Id = entityComponentReference.Entity.Id };
                        var entityComponent = (EntityComponent)Activator.CreateInstance(entityComponentReference.ComponentType);
                        entityComponent.Entity = entityReference;

                        objectContext.Instance = entityComponent;
                    }
                    else if (entityScriptReference != null)
                    {
                        var entityScript = (Script)Activator.CreateInstance(entityScriptReference.ScriptType);
                        entityScript.Id = entityScriptReference.Id;
                        var entityReference = new Entity { Id = entityScriptReference.Entity.Id };
                        entityReference.Add(new ScriptComponent { Scripts = { entityScript } });

                        objectContext.Instance = entityScript;
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
            EnterNode(ref objectContext);

            try
            {
                base.WriteYaml(ref objectContext);
            }
            finally
            {
                LeaveNode(ref objectContext);
            }
        }

        public override object ReadYaml(ref ObjectContext objectContext)
        {
            EnterNode(ref objectContext);

            try
            {
                var result = base.ReadYaml(ref objectContext);

                if (typeof(EntityGroupAssetBase).IsAssignableFrom(objectContext.Descriptor.Type))
                {
                    // Let's fixup entity references after serialization
                    EntityAnalysis.FixupEntityReferences((EntityGroupAssetBase)objectContext.Instance);
                }

                return result;
            }
            finally
            {
                LeaveNode(ref objectContext);
            }
        }

        private static void EnterNode(ref ObjectContext objectContext)
        {
            if (recursionLevel++ == 0)
                SetupMaxExpectedDepth(objectContext);

            ++levelSinceScriptComponent;
            if (objectContext.Descriptor.Type == typeof(ScriptComponent))
                levelSinceScriptComponent = 0;

            // SceneSettings: Pretend we are already inside an entity so add one level
            if (objectContext.Descriptor.Type == typeof(SceneSettings))
                recursionLevel += 2;
        }

        private static void LeaveNode(ref ObjectContext objectContext)
        {
            if (objectContext.Descriptor.Type == typeof(SceneSettings))
                recursionLevel -= 2;
            recursionLevel--;
            levelSinceScriptComponent--;
        }

        public bool CanVisit(Type type)
        {
            return typeof(EntityGroupAssetBase).IsAssignableFrom(type) || type == typeof(SceneSettings) || typeof(Entity).IsAssignableFrom(type) || typeof(EntityComponent).IsAssignableFrom(type) || typeof(Script).IsAssignableFrom(type);
        }

        //public void Visit(ref VisitorContext context)
        //{
        //    // Only visit the instance without visiting childrens
        //    context.Visitor.VisitObject(context.Instance, context.Descriptor, true);
        //}

        private static void SetupMaxExpectedDepth(ObjectContext objectContext)
        {
            // Make sure we start with 0 (in case previous serialization failed with an exception)
            if (typeof(EntityGroupAssetBase).IsAssignableFrom(objectContext.Descriptor.Type))
            {
                // Level 1 is EntityGroupAssetBase, Level 2 is EntityHierarchyData, Level 3 is Entity, Level 4 is EntityComponent
                recursionMaxExpectedDepth = 4;
            }
            else
            {
                // Level 1 is current object (Entity or EntityComponent)
                recursionMaxExpectedDepth = 2;
            }
        }
    }
}