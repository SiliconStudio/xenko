// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Collections.Generic;

using SiliconStudio.Core;
using SiliconStudio.Xenko.Engine;

namespace SiliconStudio.Xenko.Rendering.Compositing
{
    /// <summary>
    /// A collection of <see cref="CameraComponent"/>.
    /// </summary>
    [DataContract("SceneCameraSlotCollection")]
    public sealed class SceneCameraSlotCollection : List<SceneCameraSlot>
    {
        /// <summary>
        /// Property key to access the current collection of <see cref="CameraComponent"/> from <see cref="RenderContext.Tags"/>.
        /// </summary>
        public static readonly PropertyKey<SceneCameraSlotCollection> Current = new PropertyKey<SceneCameraSlotCollection>("SceneCameraSlotCollection.Current", typeof(SceneCameraSlotCollection));

        /// <summary>
        /// Gets the camera for the specified slotIndex or null if empty
        /// </summary>
        /// <param name="cameraSlotIndex">The camera slotIndex.</param>
        /// <returns>SiliconStudio.Xenko.Engine.CameraComponent.</returns>
        public CameraComponent GetCamera(SceneCameraSlotIndex cameraSlotIndex)
        {
            int index = cameraSlotIndex;
            if (index >= 0 && index < Count)
            {
                var slot = this[index];
                if (slot != null)
                {
                    return slot.Camera;
                }
            }
            return null;
        }

        /// <summary>
        /// Gets the current camera collection setup in the <see cref="RenderContext"/>.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns>SceneCameraSlotCollection.</returns>
        public static SceneCameraSlotCollection GetCurrent(RenderContext context)
        {
            return context.Tags.Get(Current);
        }
    }
}