// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using SharpYaml.Serialization;
using SharpYaml.Serialization.Serializers;

using SiliconStudio.Xenko.Engine;

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

        [ThreadStatic]
        private static int scriptLevel;

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
                    var entityScript = objectContext.Instance as Script;
                    if (entityComponent != null)
                    {
                        objectContext.Instance = new EntityComponentReference(entityComponent);
                    }
                    else if (entityScript != null && scriptLevel > 1)
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
                    else if (typeof(Script).IsAssignableFrom(type) && scriptLevel > 1)
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
            if (isSerializingAsReference)
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
            else if (typeof(EntityComponent).IsAssignableFrom(type))
            {
                componentLevel++;
            }
            else if (typeof(Script).IsAssignableFrom(type))
            {
                scriptLevel++;
            }

            isSerializingAsReference = sceneSettingsLevel > 0 || componentLevel > 1;
        }

        private static void LeaveNode(Type type)
        {
            if (type == typeof(SceneSettings))
            {
                sceneSettingsLevel--;
            }
            else if (typeof(EntityComponent).IsAssignableFrom(type))
            {
                componentLevel--;
            }
            else if (typeof(Script).IsAssignableFrom(type))
            {
                scriptLevel--;
            }

            isSerializingAsReference = sceneSettingsLevel > 0 || componentLevel > 1;
        }

        public bool CanVisit(Type type)
        {
            return typeof(EntityGroupAssetBase).IsAssignableFrom(type) || type == typeof(SceneSettings) || typeof(Entity).IsAssignableFrom(type) || typeof(EntityComponent).IsAssignableFrom(type) || typeof(Script).IsAssignableFrom(type);
        }
    }
}