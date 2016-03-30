// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Collections.Generic;
using System.Reflection;
using SiliconStudio.Xenko.Assets.Serializers;
using SiliconStudio.Xenko.Engine;

namespace SiliconStudio.Xenko.Assets.Debugging
{
    /// <summary>
    /// This class contains information about each component that must be reloaded when the game assemblies are being reloaded.
    /// </summary>
    public class ComponentToReload
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ComponentToReload"/> class.
        /// </summary>
        /// <param name="entity">The entity containing the component to reload.</param>
        /// <param name="component">The component to reload.</param>
        /// <param name="index">The index of the component to reload in the collection of components of the entity.</param>
        public ComponentToReload(Entity entity, EntityComponent component, int index)
        {
            Entity = entity;
            Component = component;
            Index = index;
        }

        public Entity Entity { get; }

        public EntityComponent Component { get; }

        public int Index { get; }

        public override string ToString()
        {
            return $"{Entity} [{Index}] {Component}";
        }
    }

    /// <summary>
    /// Helper class to reload game assemblies at runtime.
    /// </summary>
    public static class AssemblyReloader
    {
        /// <summary>
        /// Collects all the components to reload from a collection of entities.
        /// </summary>
        /// <param name="entities">The entities to process.</param>
        /// <param name="loadedAssembliesSet">The collection of assemblies containing component types thatshould be reloaded.</param>
        /// <returns>A collection of <see cref="ComponentToReload"/>.</returns>
        public static List<ComponentToReload> CollectComponentsToReload(IEnumerable<Entity> entities, HashSet<Assembly> loadedAssembliesSet)
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