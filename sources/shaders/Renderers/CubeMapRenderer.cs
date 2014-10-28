// Copyright (c) 2014 Silicon Studio Corporation (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.Effects.Modules.Processors;
using SiliconStudio.Paradox.Engine;
using SiliconStudio.Paradox.EntityModel;
using SiliconStudio.Paradox.Graphics;

namespace SiliconStudio.Paradox.Effects.Modules.Renderers
{
    /// <summary>
    /// Cubemap renderer
    /// </summary>
    public class CubeMapRenderer : RecursiveRenderer
    {
        #region Private static members

        private static readonly Vector3[] targetPositions = new Vector3[6]
        {
            Vector3.UnitX,
            -Vector3.UnitX,
            Vector3.UnitY,
            -Vector3.UnitY,
            Vector3.UnitZ,
            -Vector3.UnitZ,
        };

        private static readonly Vector3[] cameraUps = new Vector3[6]
        {
            Vector3.UnitY,
            Vector3.UnitY,
            -Vector3.UnitZ,
            Vector3.UnitZ,
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

        #region Public members

        /// <summary>
        /// Array containing each side of the cubemap as a 2D texture.
        /// </summary>
        public Texture2D[] Textures2D;

        #endregion

        #region Constructor

        /// <summary>
        /// CubeMapRenderer constructor.
        /// </summary>
        /// <param name="services">The IServiceRegistry.</param>
        /// <param name="recursivePipeline">The recursive pipeline.</param>
        /// <param name="singlePass">A flag stating if the cubemap should be rendered in one or 6 passes.</param>
        public CubeMapRenderer(IServiceRegistry services, RenderPipeline recursivePipeline, bool singlePass) : base(services, recursivePipeline)
        {
            renderInSinglePass = singlePass;
        }

        #endregion

        #region Public methods

        public override void Load()
        {
            base.Load();

            // TODO: remove this
            var cubemapSize = 128;
            Textures2D = new Texture2D[6];
            for (var i = 0; i < 6; ++i)
                Textures2D[i] = Texture2D.New(GraphicsDevice, cubemapSize, cubemapSize, PixelFormat.R8G8B8A8_UNorm, TextureFlags.ShaderResource);
        }

        public override void Unload()
        {
            // TODO: dispose targets and textures?
            base.Unload();
        }

        #endregion

        #region Protected methods

        protected override void Render(RenderContext context)
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

                    // NOTE: this is really slow so it should be avoided
                    //for (var i = 0; i < 6; ++i)
                    //    Textures2D[i].SetData(GraphicsDevice, TextureCube.GetData<uint>(i));

                    if (source.Value.GenerateMips)
                        GraphicsDevice. GenerateMips(source.Value.Texture);
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
            entity.Transformation.UpdateWorldMatrix();
            var camera = entity.Get<CameraComponent>();
            for (var i = 0; i < 6; ++i)
            {
                camera.Target.Transformation.Translation = cameraPos + targetPositions[i];
                camera.Target.Transformation.UpdateWorldMatrix();
                camera.TargetUp = cameraUps[i];
                ComputeCameraTransformations(context, camera);
                GraphicsDevice.Clear(component.DepthStencil, DepthStencilClearOptions.DepthBuffer);

                // temp
                var rt = component.Texture.ToRenderTarget(ViewType.Single, i, 0);

                GraphicsDevice.Clear(rt, Color.Black);
                GraphicsDevice.SetRenderTargets(component.DepthStencil, rt);
                base.Render(context);
            }

            GraphicsDevice.SetRenderTarget(GraphicsDevice.DepthStencilBuffer, GraphicsDevice.BackBuffer);

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
            var camera = entity.Get<CameraComponent>();
            ComputeAllCameraMatrices(context, camera);

            var rt = component.Texture.ToRenderTarget(ViewType.Full, 0, 0);

            GraphicsDevice.Clear(component.DepthStencil, DepthStencilClearOptions.DepthBuffer);
            GraphicsDevice.Clear(rt, Color.Black);
            
            GraphicsDevice.SetRenderTargets(component.DepthStencil, rt);
            
            base.Render(context);

            GraphicsDevice.SetRenderTarget(GraphicsDevice.DepthStencilBuffer, GraphicsDevice.BackBuffer);

            GraphicsDevice.Parameters.Remove(CameraCubeKeys.CameraViewProjectionMatrices);
            GraphicsDevice.Parameters.Remove(CameraCubeKeys.CameraWorldPosition);
        }

        /// <summary>
        /// Computes the parameters of the camera.
        /// </summary>
        /// <param name="context">The render context.</param>
        /// <param name="camera">The camera used to render the cubemap.</param>
        private void ComputeCameraTransformations(RenderContext context, CameraComponent camera)
        {
            //var pass = context.CurrentPass.Children[0];

            if (camera != null && camera.Entity != null)
            {
                var viewParameters = GraphicsDevice.Parameters;

                Matrix projection;
                Matrix worldToCamera;
                camera.Calculate(out projection, out worldToCamera);

                viewParameters.Set(TransformationKeys.View, worldToCamera);
                viewParameters.Set(TransformationKeys.Projection, projection);
                viewParameters.Set(CameraKeys.NearClipPlane, camera.NearPlane);
                viewParameters.Set(CameraKeys.FarClipPlane, camera.FarPlane);
                viewParameters.Set(CameraKeys.FieldOfView, camera.VerticalFieldOfView);
                viewParameters.Set(CameraKeys.Aspect, camera.AspectRatio);
                viewParameters.Set(CameraKeys.FocusDistance, camera.FocusDistance);       
            }
        }

        /// <summary>
        /// Computes the parameters of the cubemap camera.
        /// </summary>
        /// <param name="context">The render context.</param>
        /// <param name="camera">The camera used to render the cubemap.</param>
        private void ComputeAllCameraMatrices(RenderContext context, CameraComponent camera)
        {
            var cameraPos = camera.Entity.Transformation.Translation;
            camera.Entity.Transformation.UpdateWorldMatrix();
            for (var i = 0; i < 6; ++i)
            {
                camera.Target.Transformation.Translation = cameraPos + targetPositions[i];
                camera.Target.Transformation.UpdateWorldMatrix();
                camera.TargetUp = cameraUps[i];

                Matrix projection;
                Matrix worldToCamera;
                camera.Calculate(out projection, out worldToCamera);

                cameraViewProjMatrices[i] = worldToCamera * projection;
            }

            GraphicsDevice.Parameters.Set(CameraCubeKeys.CameraViewProjectionMatrices, cameraViewProjMatrices);
            GraphicsDevice.Parameters.Set(CameraCubeKeys.CameraWorldPosition, cameraPos);
        }

        #endregion
    }
}
