// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Collections.Generic;
using System.Reflection;
using SharpYaml.Events;
using SiliconStudio.Xenko.Assets.Serializers;
using SiliconStudio.Xenko.Engine;

namespace SiliconStudio.Xenko.Assets.Debugging
{
    public class ComponentToReload
    {
        public ComponentToReload(Entity entity, EntityComponent component, int index)
        {
            Entity = entity;
            Component = component;
            Index = index;
            ParsingEvents = null;
        }

        public Entity Entity { get; }

        public EntityComponent Component { get; }

        public int Index { get; }

        public List<ParsingEvent> ParsingEvents { get; set; }
    }

    public class ReloadedComponentEntry
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

    /// <summary>
    /// Helper class to reload game assemblies at runtime.
    /// </summary>
    public static class AssemblyReloader
    {
        /// <summary>
        /// Collects all the component to reload from a collection of entities, 
        /// </summary>
        /// <param name="entities"></param>
        /// <param name="loadedAssembliesSet"></param>
        /// <returns></returns>
        public static List<ComponentToReload> CollectComponentsToReload(List<Entity> entities, HashSet<Assembly> loadedAssembliesSet)
        {
            var result = new List<ComponentToReload>();

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

                    // TODO: Serialize Scene script too (async?) -- doesn't seem necessary even for complex cases
                    // (i.e. referencing assets, entities and/or scripts) but still a ref counting check might be good
                    result.Add(new ComponentToReload(entity, component, index));
                }
            }
            return result;
        }
    }
}