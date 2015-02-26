// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;

using SiliconStudio.Core;
using SiliconStudio.Paradox.Graphics;

namespace SiliconStudio.Paradox.EntityModel
{
    /// <summary>
    /// The base class to generate gizmo-entity associated to a scene's <see cref="Entity"/>.
    /// </summary>
    public abstract class GizmoEntityFactory: ComponentBase
    {
        private IGraphicsDeviceService graphicsDeviceService;

        /// <summary>
        /// Gets the graphics device.
        /// </summary>
        /// <value>The graphics device.</value>
        public GraphicsDevice GraphicsDevice
        {
            get
            {
                if (graphicsDeviceService == null)
                {
                    throw new InvalidOperationException("GraphicsDeviceService is not yet initialized");
                }

                return graphicsDeviceService.GraphicsDevice;
            }
        }

        /// <summary>
        /// Initialize the <see cref="GizmoEntityFactory"/>
        /// </summary>
        /// <param name="services">The list of services of the scene</param>
        public virtual void Initialize(IServiceRegistry services)
        {
            graphicsDeviceService = services.GetServiceAs<IGraphicsDeviceService>();
        }

        /// <summary>
        /// Generates the gizmo-entity to associate to the provided entity.
        /// </summary>
        /// <param name="component">The component that requires the gizmo-entity</param>
        /// <returns>The gizmo-entity</returns>
        public abstract Entity CreateGizmoEntity(EntityComponent component);
    }

    /// <summary>
    /// The generic base class to generate gizmo-entity associated to a scene's <see cref="Entity"/>.
    /// </summary>
    public abstract class GizmoEntityFactory<T> : GizmoEntityFactory
        where T : EntityComponent
    {
        public override Entity CreateGizmoEntity(EntityComponent component)
        {
            return CreateGizmoEntity((T)component);
        }

        /// <summary>
        /// Generates the gizmo-entity to associate to the provided entity.
        /// </summary>
        /// <param name="component">The component that requires the gizmo-entity</param>
        /// <returns>The gizmo-entity</returns>
        public abstract Entity CreateGizmoEntity(T component);
    }
}