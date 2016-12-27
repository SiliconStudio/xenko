// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;

namespace SiliconStudio.Xenko.Engine
{
    /// <summary>
    /// Internal interface used to notify component changed in <see cref="Entity.Components"/> to <see cref="EntityManager"/>.
    /// </summary>
    public interface IEntityComponentNotify
    {
        /// <summary>
        /// Called when a component changed on the entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="index">The index in the <see cref="Entity.Components"/>.</param>
        /// <param name="oldComponent">The old component (may be null if newComponent is added)</param>
        /// <param name="newComponent">The new component (may be null if oldComponent is removed)</param>
        void OnComponentChanged(Entity entity, int index, EntityComponent oldComponent, EntityComponent newComponent);

        /// <summary>
        /// To be invoked when the whole global hierarchy has changed
        /// </summary>
        /// <param name="entity">The entity that has changed</param>
        void OnHierarchyChanged(Entity entity);

        /// <summary>
        /// Listens when the whole global hierarchy has changed
        /// </summary>
        event EventHandler<Entity> HierarchyChanged;
    }
}