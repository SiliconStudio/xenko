// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Rendering.Composers;

namespace SiliconStudio.Xenko.Rendering
{
    public static class CameraComponentRendererExtensions
    {
        /// <summary>
        /// Property key to access the current collection of <see cref="CameraComponent"/> from <see cref="RenderContext.Tags"/>.
        /// </summary>
        public static readonly PropertyKey<CameraComponent> Current = new PropertyKey<CameraComponent>("CameraComponentRenderer.CurrentCamera", typeof(CameraComponent));

        public static CameraComponent GetCameraFromSlot(this RenderContext context, SceneCameraSlotIndex cameraSlotIndex)
        {
            var cameraCollection = SceneCameraSlotCollection.GetCurrent(context);
            if (cameraCollection == null)
            {
                return null;
            }

            // If no camera found, just skip this part.
            var camera = cameraCollection.GetCamera(cameraSlotIndex);
            return camera;
        }

        public static CameraComponent GetCurrentCamera(this RenderContext context)
        {
            return context.Tags.Get(Current);
        }
    }
}