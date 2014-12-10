// Copyright (c) 2014 Silicon Studio Corporation (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;

using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.Effects.Processors;
using SiliconStudio.Paradox.Engine;
using SiliconStudio.Paradox.EntityModel;
using SiliconStudio.Paradox.Graphics;

namespace SiliconStudio.Paradox.Effects.Cubemap
{
    /// <summary>
    /// Render dynamic cubemaps.
    /// </summary>
    public class CubemapRenderer : RecursiveRenderer
    {
        #region Private static members

        private static readonly Vector3[] TargetPositions = new Vector3[6]
        {
            Vector3.UnitX,
            -Vector3.UnitX,
            Vector3.UnitY,
            -Vector3.UnitY,
            // since camera matrices will flip the z coordinate, we need to swap the Z and -Z faces render target.
            -Vector3.UnitZ,
            Vector3.UnitZ,
        };

        private static readonly Vector3[] CameraUps = new Vector3[6]
        {
            Vector3.UnitY,
            Vector3.UnitY,
            Vector3.UnitZ,
            -Vector3.UnitZ,
            Vector3.UnitY,
            Vector3.UnitY
        };

        #endregion

        #region Private members

        // camera parameters
        private Matrix[] cameraViewProjMatrices = new Matrix[6];

        // flag to render in a single pass
        private bool renderInSinglePass;

        #endregion

        #region Constructor

        /// <summary>
        /// CubemapRenderer constructor.
        /// </summary>
        /// <param name="services">The IServiceRegistry.</param>
        /// <param name="recursivePipeline">The recursive pipeline.</param>
        /// <param name="singlePass">A flag stating if the cubemap should be rendered in one or 6 passes.</param>
        public CubemapRenderer(IServiceRegistry services, RenderPipeline recursivePipeline, bool singlePass) : base(services, recursivePipeline)
        {
            // temporary
            if (GraphicsDevice.Features.Profile < GraphicsProfile.Level_10_1)
                throw new Exception("Cubemaps not supported on profiles below 10.1.");
            renderInSinglePass = singlePass && (GraphicsDevice.Features.Profile >= GraphicsProfile.Level_10_1);
        }

        #endregion

        #region Protected methods

        protected override void OnRendering(RenderContext context)
        {
            var entitySystem = Services.GetServiceAs<EntitySystem>();
            var cubemapSourceProcessor = entitySystem.GetProcessor<CubemapSourceProcessor>();
            if (cubemapSourceProcessor == null)
                return;

            foreach (var source in cubemapSourceProcessor.Cubemaps)
            {
                if (source.Value.IsDynamic)
                {
                    if (renderInSinglePass)
                        RenderInSinglePass(context, source.Key, source.Value);
                    else
                        RenderInSixPasses(context, source.Key, source.Value);
                }
            }
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Renders the cubemap in 6 passes, one for each face.
        /// </summary>
        /// <param name="context">The render context.</param>
        /// <param name="entity">The entity the cubemap is attached to.</param>
        /// <param name="component">The CubemapSource component.</param>
        private void RenderInSixPasses(RenderContext context, Entity entity, CubemapSourceComponent component)
        {
            var cameraPos = entity.Transformation.Translation;
            for (var i = 0; i < 6; ++i)
            {
                Matrix worldToCamera;
                Matrix projection;
                ComputeViewProjectionMatrices(cameraPos, TargetPositions[i], CameraUps[i], component, out worldToCamera, out projection);

                // TODO: set parameters on another collection?
                GraphicsDevice.Parameters.Set(TransformationKeys.View, worldToCamera);
                GraphicsDevice.Parameters.Set(TransformationKeys.Projection, projection);
                GraphicsDevice.Parameters.Set(CameraKeys.NearClipPlane, component.NearPlane);
                GraphicsDevice.Parameters.Set(CameraKeys.FarClipPlane, component.FarPlane);
                GraphicsDevice.Parameters.Set(CameraKeys.FieldOfView, MathUtil.PiOverTwo);
                GraphicsDevice.Parameters.Set(CameraKeys.Aspect, 1);

                if (component.RenderTargets == null)
                    component.CreateSingleViewRenderTargets();

                var renderTarget = component.RenderTargets[i];
                GraphicsDevice.Clear(component.DepthStencil, DepthStencilClearOptions.DepthBuffer);
                GraphicsDevice.Clear(renderTarget, Color.Black);

                GraphicsDevice.SetDepthAndRenderTargets(component.DepthStencil, renderTarget);
                
                // TODO: flip face culling

                base.OnRendering(context);
            }

            GraphicsDevice.SetDepthAndRenderTarget(GraphicsDevice.DepthStencilBuffer, GraphicsDevice.BackBuffer);

            GraphicsDevice.Parameters.Remove(TransformationKeys.View);
            GraphicsDevice.Parameters.Remove(TransformationKeys.Projection);
            GraphicsDevice.Parameters.Remove(CameraKeys.NearClipPlane);
            GraphicsDevice.Parameters.Remove(CameraKeys.FarClipPlane);
            GraphicsDevice.Parameters.Remove(CameraKeys.FieldOfView);
            GraphicsDevice.Parameters.Remove(CameraKeys.Aspect);
            GraphicsDevice.Parameters.Remove(CameraKeys.FocusDistance);
        }

        /// <summary>
        /// Renders the cubemap in one pass using a geometry shader.
        /// </summary>
        /// <param name="context">The render context.</param>
        /// <param name="entity">The entity the cubemap is attached to.</param>
        /// <param name="component">The CubemapSource component.</param>
        private void RenderInSinglePass(RenderContext context, Entity entity, CubemapSourceComponent component)
        {
            var cameraPos = component.Entity.Transformation.Translation;
            for (var i = 0; i < 6; ++i)
            {
                Matrix worldToCamera;
                Matrix projection;
                ComputeViewProjectionMatrices(cameraPos, TargetPositions[i], CameraUps[i], component, out worldToCamera, out projection);
                cameraViewProjMatrices[i] = worldToCamera * projection;
            }

            // TODO: set parameters on another collection?
            GraphicsDevice.Parameters.Set(CameraCubeKeys.CameraViewProjectionMatrices, cameraViewProjMatrices);
            GraphicsDevice.Parameters.Set(CameraCubeKeys.CameraWorldPosition, cameraPos);

            if (component.RenderTarget == null)
                component.CreateFullViewRenderTarget();

            GraphicsDevice.Clear(component.DepthStencil, DepthStencilClearOptions.DepthBuffer);
            GraphicsDevice.Clear(component.RenderTarget, Color.Black);
            
            GraphicsDevice.SetDepthAndRenderTargets(component.DepthStencil, component.RenderTarget);
            
            base.OnRendering(context);

            GraphicsDevice.SetDepthAndRenderTarget(GraphicsDevice.DepthStencilBuffer, GraphicsDevice.BackBuffer);

            GraphicsDevice.Parameters.Remove(CameraCubeKeys.CameraViewProjectionMatrices);
            GraphicsDevice.Parameters.Remove(CameraCubeKeys.CameraWorldPosition);
        }

        #endregion

        #region Helpers

        private static void ComputeViewProjectionMatrices(Vector3 position, Vector3 faceOffset, Vector3 up, CubemapSourceComponent source, out Matrix viewMatrix, out Matrix projection)
        {
            viewMatrix = Matrix.LookAtRH(position, position + faceOffset, up);
            Matrix.PerspectiveFovRH(MathUtil.PiOverTwo, 1, source.NearPlane, source.FarPlane, out projection);
        }

        #endregion
    }
}
