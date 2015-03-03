// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;

namespace SiliconStudio.Paradox.EntityModel
{
    /// <summary>
    /// The base class to generate gizmo-entity associated to a scene's <see cref="Entity"/>.
    /// </summary>
    public abstract class GizmoEntityFactory: ComponentBase
    {
        /// <summary>
        /// Generates the gizmo-entity to associate to the provided entity.
        /// </summary>
        /// <param name="sceneEntity">The scene entity that requires a gizmo entity</param>
        /// <param name="component">The component that requires the gizmo-entity</param>
        /// <returns>The gizmo-entity</returns>
        public abstract IGizmo CreateGizmoEntity(Entity sceneEntity, EntityComponent component);
    }

    /// <summary>
    /// The generic base class to generate gizmo-entity associated to a scene's <see cref="Entity"/>.
    /// </summary>
    public abstract class GizmoEntityFactory<T> : GizmoEntityFactory
        where T : EntityComponent
    {
        public override IGizmo CreateGizmoEntity(Entity sceneEntity, EntityComponent component)
        {
            return CreateGizmoEntity(sceneEntity, (T)component);
        }

        /// <summary>
        /// Generates the gizmo-entity to associate to the provided entity.
        /// </summary>
        /// <param name="sceneEntity">The scene entity that requires the gizmo-entity</param>
        /// <param name="component">The component that requires the gizmo-entity</param>
        /// <returns>The gizmo-entity</returns>
        public abstract IGizmo CreateGizmoEntity(Entity sceneEntity, T component);
    }
}