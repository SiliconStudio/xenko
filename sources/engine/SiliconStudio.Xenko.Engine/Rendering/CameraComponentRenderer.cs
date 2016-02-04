// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;

using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Rendering;

namespace SiliconStudio.Xenko.Rendering
{
    /// <summary>
    /// Updates the parameters in the 
    /// </summary>
    public class CameraComponentRenderer : EntityComponentRendererBase
    {
        /// <summary>
        /// Property key to access the current collection of <see cref="CameraComponent"/> from <see cref="RenderContext.Tags"/>.
        /// </summary>
        public static readonly PropertyKey<CameraComponent> Current = new PropertyKey<CameraComponent>("CameraComponentRenderer.CurrentCamera", typeof(CameraComponent));

        public override bool SupportPicking { get { return true; } }

        protected override void PrepareCore(RenderDrawContext context, RenderItemCollection opaqueList, RenderItemCollection transparentList)
        {
            var cameraState = context.RenderContext.GetCurrentCamera();

            if (cameraState == null)
                return;

            UpdateParameters(context, cameraState);
        }

        protected override void DrawCore(RenderDrawContext context, RenderItemCollection renderItems, int fromIndex, int toIndex)
        {
            // Nothing to draw for this camera
        }

        public static void UpdateParameters(RenderDrawContext context, CameraComponent camera)
        {
            if (camera == null) throw new ArgumentNullException("camera");

            // Setup viewport size
            var currentViewport = context.CommandList.Viewport;
            var aspectRatio = currentViewport.AspectRatio;

            // Update the aspect ratio
            if (camera.UseCustomAspectRatio)
            {
                aspectRatio = camera.AspectRatio;
            }

            // If the aspect ratio is calculated automatically from the current viewport, update matrices here
            camera.Update(aspectRatio);

            // TODO GRAPHICS REFACTOR move that to UpdateCameraToRenderView and TransformRenderFeature
            // Store the current view/projection matrix in the context
            //var viewParameters = context.Parameters;
            //viewParameters.Set(TransformationKeys.View, camera.ViewMatrix);
            //viewParameters.Set(TransformationKeys.Projection, camera.ProjectionMatrix);
            //viewParameters.Set(TransformationKeys.ViewProjection, camera.ViewProjectionMatrix);
            //viewParameters.Set(CameraKeys.NearClipPlane, camera.NearClipPlane);
            //viewParameters.Set(CameraKeys.FarClipPlane, camera.FarClipPlane);
            //viewParameters.Set(CameraKeys.VerticalFieldOfView, camera.VerticalFieldOfView);
            //viewParameters.Set(CameraKeys.OrthoSize, camera.OrthographicSize);
            //viewParameters.Set(CameraKeys.ViewSize, new Vector2(currentViewport.Width, currentViewport.Height));
            //viewParameters.Set(CameraKeys.AspectRatio, aspectRatio);

            //viewParameters.Set(CameraKeys.FocusDistance, camera.FocusDistance);
        }
    }
}