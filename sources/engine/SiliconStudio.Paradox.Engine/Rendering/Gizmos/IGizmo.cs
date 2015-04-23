// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using SiliconStudio.Paradox.Engine;

namespace SiliconStudio.Paradox.Rendering.Gizmos
{
    /// <summary>
    /// The interface for scene editor's gizmos
    /// </summary>
    public interface IGizmo : IDisposable
    {
        /// <summary>
        /// Gets or sets the selected state of the gizmo.
        /// </summary>
        bool IsSelected { get; set; }

        /// <summary>
        /// Gets or sets the enabled state of the gizmo.
        /// </summary>
        bool IsEnabled { get; set; }

        /// <summary>
        /// Gets the entity of the scene associated to the gizmo.
        /// </summary>
        Entity SceneEntity { get; }

        /// <summary>
        /// Indicate if the mouse is over the gizmo.
        /// </summary>
        /// <param name="context">The gizmo context</param>
        /// <returns><value>True</value> if the mouse is over the gizmo</returns>
        bool IsUnderMouse(GizmoContext context);

        /// <summary>
        /// Initialize the gizmo.
        /// </summary>
        /// <param name="sceneInstance">The instance of the gizmo scene</param>
        /// <param name="sceneEntity">The entity of the scene associated to the gizmo</param>
        void Initialize(SceneInstance sceneInstance, Entity sceneEntity);

        /// <summary>
        /// Update the gizmo state.
        /// </summary>
        /// <param name="context">The gizmo context</param>
        void Update(GizmoContext context);

        /// <summary>
        /// Prepare the gizmo the coming draw.
        /// </summary>
        /// <param name="context"></param>
        void PrepareDraw(RenderContext context);
    }
}