// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

namespace SiliconStudio.Paradox.EntityModel
{    
    /// <summary>
    /// The update context for the gizmos.
    /// </summary>
    public class GizmoContext
    {
        /// <summary>
        /// The scale unit of the current scene
        /// </summary>
        public readonly float SceneUnit;

        /// <summary>
        /// Indicate if the user is current multi-selecting entities.
        /// </summary>
        public readonly bool IsMultiSelecting;

        /// <summary>
        /// Indicate if the camera is currently navigating in the space.
        /// </summary>
        public readonly bool IsCameraMoving;

        /// <summary>
        /// The id of the entity component picked since last update.
        /// </summary>
        public readonly int PickedComponentId;

        public GizmoContext(float sceneUnit, bool isMultiSelecting, bool isCameraMoving, int pickedComponentId)
        {
            SceneUnit = sceneUnit;
            IsCameraMoving = isCameraMoving;
            IsMultiSelecting = isMultiSelecting;
            PickedComponentId = pickedComponentId;
        }
    };
}