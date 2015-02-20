// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.Engine;
using SiliconStudio.Paradox.Engine.Graphics;

namespace SiliconStudio.Paradox.Effects
{
    /// <summary>
    /// Updates the parameters in the 
    /// </summary>
    public class CameraComponentRenderer : EntityComponentRendererBase
    {
        protected override void DrawCore(RenderContext context)
        {
            var camera = CameraComponent.GetCurrent(context);

            if (camera == null || camera.Entity == null)
                return;

            var viewParameters = context.Parameters;

            Matrix projection;
            Matrix worldToCamera;
            camera.Calculate(out projection, out worldToCamera);

            viewParameters.Set(TransformationKeys.View, worldToCamera);
            viewParameters.Set(TransformationKeys.Projection, projection);
            viewParameters.Set(CameraKeys.NearClipPlane, camera.NearPlane);
            viewParameters.Set(CameraKeys.FarClipPlane, camera.FarPlane);
            if (camera.Projection is CameraProjectionPerspective)
            {
                viewParameters.Set(CameraKeys.FieldOfView, ((CameraProjectionPerspective)camera.Projection).VerticalFieldOfView);
            } else if (camera.Projection is CameraProjectionOrthographic)
            {
                viewParameters.Set(CameraKeys.OrthoSize, ((CameraProjectionOrthographic)camera.Projection).Size);
            }
            viewParameters.Set(CameraKeys.Aspect, camera.AspectRatio);
            viewParameters.Set(CameraKeys.FocusDistance, camera.FocusDistance);   
        }
    }
}