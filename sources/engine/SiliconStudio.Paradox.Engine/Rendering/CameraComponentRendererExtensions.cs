// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Paradox.Engine;
using SiliconStudio.Paradox.Rendering.Composers;

namespace SiliconStudio.Paradox.Rendering
{
    public static class CameraComponentRendererExtensions
    {
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
            return context.Tags.Get(CameraComponentRenderer.Current);
        }
    }
}