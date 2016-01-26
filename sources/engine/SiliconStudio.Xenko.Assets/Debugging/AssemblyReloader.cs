// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Collections.Generic;
using System.Reflection;
using SharpYaml.Events;
using SiliconStudio.Core.Diagnostics;
using SiliconStudio.Xenko.Assets.Serializers;
using SiliconStudio.Xenko.Engine;

namespace SiliconStudio.Xenko.Assets.Debugging
{
    /// <summary>
    /// Helper to reload game assemblies at runtime. It will update currently running scripts.
    /// </summary>
    public abstract class AssemblyReloader
    {
        protected ILogger log;
        protected readonly List<Entity> entities = new List<Entity>();

        protected List<ReloadedComponentEntry> CollectReloadedComponentEntries(HashSet<Assembly> loadedAssembliesSet)
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

        protected abstract EntityComponent DeserializeComponent(ReloadedComponentEntry reloadedComponent);

        protected abstract List<ParsingEvent> SerializeComponent(EntityComponent component);

        protected abstract ReloadedComponentEntry CreateReloadedComponentEntry(Entity entity, int index, List<ParsingEvent> parsingEvents, EntityComponent component);

        protected abstract void ReplaceComponent(ReloadedComponentEntry reloadedComponent);

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