// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SharpYaml;
using SharpYaml.Events;
using SharpYaml.Serialization;
using SiliconStudio.Core;
using SiliconStudio.Core.Reflection;
using SiliconStudio.Core.Serialization;
using SiliconStudio.Core.Yaml;
using SiliconStudio.Xenko.Assets.Debugging;
using SiliconStudio.Xenko.Debugger.Target;
using SiliconStudio.Xenko.Engine;

namespace SiliconStudio.Xenko.Debugger
{
    public static class LiveAssemblyReloader
    {
        public static void Reload(Game game, AssemblyContainer assemblyContainer, List<Assembly> assembliesToUnregister, List<Assembly> assembliesToRegister)
        {
            List<Entity> entities = new List<Entity>();

            if (game != null)
                entities.AddRange(game.SceneSystem.SceneInstance);

            CloneReferenceSerializer.References = new List<object>();

            var loadedAssembliesSet = new HashSet<Assembly>(assembliesToUnregister);
            var reloadedComponents = new List<ReloadedComponentEntryLive>();
            var componentsToReload = AssemblyReloader.CollectComponentsToReload(entities, loadedAssembliesSet);
            foreach (var componentToReload in componentsToReload)
            {
                var parsingEvents = SerializeComponent(componentToReload.Component);
                // TODO: Serialize Scene script too (async?) -- doesn't seem necessary even for complex cases
                // (i.e. referencing assets, entities and/or scripts) but still a ref counting check might be good
                reloadedComponents.Add(new ReloadedComponentEntryLive(componentToReload, parsingEvents));
            }

            foreach (var assembly in assembliesToUnregister)
            {
                // Unregisters assemblies that have been registered in Package.Load => Package.LoadAssemblyReferencesForPackage
                AssemblyRegistry.Unregister(assembly);

                // Unload binary serialization
                DataSerializerFactory.UnregisterSerializationAssembly(assembly);

                // Unload assembly
                assemblyContainer.UnloadAssembly(assembly);
            }

            foreach (var assembly in assembliesToRegister)
            {
                ModuleRuntimeHelpers.RunModuleConstructor(assembly.ManifestModule);

                // Unregisters assemblies that have been registered in Package.Load => Package.LoadAssemblyReferencesForPackage
                AssemblyRegistry.Register(assembly, AssemblyCommonCategories.Assets);

                DataSerializerFactory.RegisterSerializationAssembly(assembly);
            }

            // First pass of deserialization: recreate the scripts
            foreach (ReloadedComponentEntryLive reloadedScript in reloadedComponents)
            {
                // Try to create object
                var objectStart = reloadedScript.YamlEvents.OfType<MappingStart>().FirstOrDefault();
                if (objectStart != null)
                {
                    // Get type info
                    var objectStartTag = objectStart.Tag;
                    bool alias;
                    var componentType = YamlSerializer.GetSerializerSettings().TagTypeRegistry.TypeFromTag(objectStartTag, out alias);
                    if (componentType != null)
                    {
                        reloadedScript.NewComponent = (EntityComponent)Activator.CreateInstance(componentType);
                    }
                }
            }

            // Second pass: update script references in live objects
            // As a result, any script references processed by Yaml serializer will point to updated objects (script reference cycle will work!)
            for (int index = 0; index < CloneReferenceSerializer.References.Count; index++)
            {
                var component = CloneReferenceSerializer.References[index] as EntityComponent;
                if (component != null)
                {
                    var reloadedComponent = reloadedComponents.FirstOrDefault(x => x.OriginalComponent == component);
                    if (reloadedComponent != null)
                    {
                        CloneReferenceSerializer.References[index] = reloadedComponent.NewComponent;
                    }
                }
            }

            // Third pass: deserialize
            reloadedComponents.ForEach(x => ReplaceComponent(game, x));

            CloneReferenceSerializer.References = null;
        }

        private static EntityComponent DeserializeComponent(ReloadedComponentEntryLive reloadedComponent)
        {
            var eventReader = new EventReader(new MemoryParser(reloadedComponent.YamlEvents));
            var components = new EntityComponentCollection();

            // Use the newly created component during second pass for proper cycle deserialization
            var newComponent = reloadedComponent.NewComponent;
            if (newComponent != null)
                components.Add(newComponent);

            // Try to create component first
            YamlSerializer.Deserialize(eventReader, components, typeof(EntityComponentCollection));
            var component = components.Count == 1 ? components[0] : null;
            return component;
        }

        private static List<ParsingEvent> SerializeComponent(EntityComponent component)
        {
            // Wrap component in a EntityComponentCollection to properly handle errors
            var components = new Entity { component }.Components;

            // Serialize with Yaml layer
            var parsingEvents = new List<ParsingEvent>();
            // We also want to serialize live component variables
            var serializerContextSettings = new SerializerContextSettings { MemberMask = DataMemberAttribute.DefaultMask | ScriptComponent.LiveScriptingMask };
            YamlSerializer.Serialize(new ParsingEventListEmitter(parsingEvents), components, typeof(EntityComponentCollection), serializerContextSettings);
            return parsingEvents;
        }

        private static void ReplaceComponent(Game game, ReloadedComponentEntryLive reloadedComponent)
        {
            // Create new component instance
            var newComponent = DeserializeComponent(reloadedComponent);

            // Dispose and unregister old component (and their MicroThread, if any)
            var oldComponent = reloadedComponent.Entity.Components[reloadedComponent.ComponentIndex];

            // Flag scripts as being live reloaded
            if (game != null && oldComponent is ScriptComponent)
            {
                game.Script.LiveReload((ScriptComponent)oldComponent, (ScriptComponent)newComponent);
            }

            // Replace with new component
            // TODO: Remove component before serializing it, so cancellation code can run
            reloadedComponent.Entity.Components[reloadedComponent.ComponentIndex] = newComponent;

            // TODO: Dispose or Cancel on script?
            (oldComponent as ScriptComponent)?.Cancel();
        }

        private class ReloadedComponentEntryLive
        {
            private readonly ComponentToReload componentToReload;

            public ReloadedComponentEntryLive(ComponentToReload componentToReload, List<ParsingEvent> parsingEvents)
            {
                this.componentToReload = componentToReload;
                YamlEvents = parsingEvents;
            }

            public Entity Entity => componentToReload.Entity;

            public int ComponentIndex => componentToReload.Index;

            public readonly List<ParsingEvent> YamlEvents;

            public EntityComponent OriginalComponent => componentToReload.Component;

            public EntityComponent NewComponent { get; set; }
        }
    }
}