// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;

using SiliconStudio.Core;
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
        /// <summary>
        /// Property key to access the current collection of <see cref="CameraComponent"/> from <see cref="RenderContext.Tags"/>.
        /// </summary>
        public static readonly PropertyKey<CameraComponentState> Current = new PropertyKey<CameraComponentState>("CameraComponentRenderer.CurrentCamera", typeof(CameraComponentState));

        protected override void DrawCore(RenderContext context)
        {
            var cameraState = context.Tags.Get(Current);

            if (cameraState == null)
                return;

            UpdateParameters(context.Parameters, cameraState);
        }

        public static void UpdateParameters(ParameterCollection viewParameters, CameraComponentState cameraState)
        {
            if (viewParameters == null) throw new ArgumentNullException("viewParameters");
            if (cameraState == null) throw new ArgumentNullException("cameraState");
        
            if (cameraState.CameraComponent.Entity == null)
                return;

            var camera = cameraState.CameraComponent;

            Matrix projection = cameraState.Projection;
            Matrix worldToCamera = cameraState.View;

            viewParameters.Set(TransformationKeys.View, worldToCamera);
            viewParameters.Set(TransformationKeys.Projection, projection);
            viewParameters.Set(CameraKeys.NearClipPlane, camera.NearPlane);
            viewParameters.Set(CameraKeys.FarClipPlane, camera.FarPlane);
            if (camera.Projection == CameraProjectionMode.Perspective)
            {
                viewParameters.Set(CameraKeys.FieldOfView, camera.VerticalFieldOfView);
            }
            else
            {
                viewParameters.Set(CameraKeys.OrthoSize, camera.OrthographicSize);
            }
            viewParameters.Set(CameraKeys.Aspect, camera.AspectRatio);
            viewParameters.Set(CameraKeys.FocusDistance, camera.FocusDistance);
        }
    }
}