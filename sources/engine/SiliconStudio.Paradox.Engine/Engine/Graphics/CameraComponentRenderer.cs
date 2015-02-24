// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;

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

        protected override void PrepareCore(RenderContext context, RenderItemCollection opaqueList, RenderItemCollection transparentList)
        {
            var cameraState = context.Tags.Get(Current);

            if (cameraState == null)
                return;

            UpdateParameters(context, cameraState);
        }

        protected override void DrawCore(RenderContext context, RenderItemCollection renderItems, int fromIndex, int toIndex)
        {
            // Nothing to draw for this camera
        }

        public static void UpdateParameters(RenderContext context, CameraComponentState cameraState)
        {
            if (cameraState == null) throw new ArgumentNullException("cameraState");

            ParameterCollection viewParameters = context.Parameters;

            if (cameraState.CameraComponent.Entity == null)
                return;

            var camera = cameraState.CameraComponent;

            // Store the current view/projection matrix in the context
            context.ProjectionMatrix = cameraState.Projection;
            context.ViewMatrix = cameraState.View;
            Matrix.Multiply(ref context.ViewMatrix, ref context.ProjectionMatrix, out context.ViewProjectionMatrix);

            viewParameters.Set(TransformationKeys.View, context.ViewMatrix);
            viewParameters.Set(TransformationKeys.Projection, context.ProjectionMatrix);
            viewParameters.Set(TransformationKeys.ViewProjection, context.ViewProjectionMatrix);

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