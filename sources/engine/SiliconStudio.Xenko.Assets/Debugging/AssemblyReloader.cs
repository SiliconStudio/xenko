// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SharpYaml;
using SharpYaml.Events;
using SharpYaml.Serialization;
using SiliconStudio.Assets.Serializers;
using SiliconStudio.Core.Diagnostics;
using SiliconStudio.Core.Yaml;
using SiliconStudio.Xenko.Assets.Serializers;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Engine.Design;

namespace SiliconStudio.Xenko.Assets.Debugging
{
    /// <summary>
    /// Helper to reload game assemblies at runtime. It will update currently running scripts.
    /// </summary>
    public abstract class AssemblyReloader
    {
        protected ILogger log;
        protected readonly List<Entity> entities = new List<Entity>();

        protected virtual void RestoreReloadedComponentEntries(List<ReloadedComponentEntry> reloadedComponents)
        {
            foreach (var reloadedComponent in reloadedComponents)
            {
                var componentToReload = reloadedComponent.Entity.Components[reloadedComponent.ComponentIndex];
                ReplaceComponent(componentToReload, reloadedComponent);
            }
        }

        protected virtual List<ReloadedComponentEntry> CollectReloadedComponentEntries(HashSet<Assembly> loadedAssembliesSet)
        {
            var reloadedScripts = new List<ReloadedComponentEntry>();

            // Find components that will need reloading
            foreach (var entity in entities)
            {
                for (int index = 0; index < entity.Components.Count; index++)
                {
                    var component = entity.Components[index];

                    var componentType = component.GetType();

                    // We force both scripts that were just unloaded and UnloadableComponent (from previous failure) to try to reload
                    if (!loadedAssembliesSet.Contains(componentType.Assembly) && componentType != typeof(UnloadableComponent))
                        continue;

                    var parsingEvents = SerializeComponent(component);

                    // TODO: Serialize Scene script too (async?) -- doesn't seem necessary even for complex cases
                    // (i.e. referencing assets, entities and/or scripts) but still a ref counting check might be good

                    reloadedScripts.Add(CreateReloadedComponentEntry(entity, index, parsingEvents, component));
                }
            }
            return reloadedScripts;
        }

        protected virtual EntityComponent DeserializeComponent(ReloadedComponentEntry reloadedComponent)
        {
            // Use an entity to deserialize the component 
            var entity = new Entity();
            entity.Components.Clear();
            
            var eventReader = new EventReader(new MemoryParser(reloadedComponent.YamlEvents));
            YamlSerializer.Deserialize(eventReader, entity, typeof(Entity), log != null ? new SerializerContextSettings { Logger = new YamlForwardLogger(log) } : null);
            var component = entity.Components.FirstOrDefault();
            entity.Components.Clear();
            return component;
        }

        protected virtual List<ParsingEvent> SerializeComponent(EntityComponent component)
        {
            // Use an entity to deserialize the component 

            // In order to do this, we need to save the parent entity of the component being deserialized
            var previousEntity = component.Entity;
            component.Entity = null;

            var entity = new Entity();
            entity.Components.Clear();
            entity.Components.Add(component);

            // Serialize with Yaml layer
            var parsingEvents = new List<ParsingEvent>();
            YamlSerializer.Serialize(new ParsingEventListEmitter(parsingEvents), entity, typeof(Entity));

            entity.Components.Clear();
            component.Entity = previousEntity;

            return parsingEvents;
        }

        protected virtual ReloadedComponentEntry CreateReloadedComponentEntry(Entity entity, int index, List<ParsingEvent> parsingEvents, EntityComponent component)
        {
            return new ReloadedComponentEntry(entity, index, parsingEvents);
        }

        protected abstract void ReplaceComponent(EntityComponent entityComponent, ReloadedComponentEntry reloadedComponent);

        protected class ReloadedComponentEntry
        {
            public readonly Entity Entity;
            public readonly int ComponentIndex;
            public readonly List<ParsingEvent> YamlEvents;

            public ReloadedComponentEntry(Entity entity, int componentIndex, List<ParsingEvent> yamlEvents)
            {
                Entity = entity;
                ComponentIndex = componentIndex;
                YamlEvents = yamlEvents;
            }
        }
    }
}