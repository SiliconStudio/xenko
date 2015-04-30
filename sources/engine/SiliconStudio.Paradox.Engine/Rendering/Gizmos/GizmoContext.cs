// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

namespace SiliconStudio.Paradox.Rendering.Gizmos
{    
    /// <summary>
    /// The update context for the gizmos.
    /// </summary>
    public class GizmoContext
    {
        private const float SceneUnitFactor = 0.5f;

        /// <summary>
        /// The scale unit of the current scene
        /// </summary>
        public float SceneUnit { get; set; }

        /// <summary>
        /// Indicate if the user is current multi-selecting entities.
        /// </summary>
        public bool IsMultiSelecting { get; set; }

        /// <summary>
        /// Indicate if the camera is currently navigating in the space.
        /// </summary>
        public bool IsCameraMoving { get; set; }

        /// <summary>
        /// The id of the entity component picked since last update.
        /// </summary>
        public int PickedComponentId { get; set; }

        public GizmoContext(float sceneUnit, bool isMultiSelecting, bool isCameraMoving)
        {
            SceneUnit = sceneUnit * SceneUnitFactor;
            IsCameraMoving = isCameraMoving;
            IsMultiSelecting = isMultiSelecting;
        }
    };
}