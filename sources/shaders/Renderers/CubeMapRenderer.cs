// Copyright (c) 2014 Silicon Studio Corporation (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
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

        // cubemap size
        private int cubemapSize;

        // camera parameters
        private Matrix[] cameraViewProjMatrices = new Matrix[6];

        // for 6 passes
        private RenderTarget[] cubeMapRenderTargetsArray;

        // for single pass
        private RenderTarget cubeMapRenderTarget;

        // depth stencil buffer
        private DepthStencilBuffer cubeMapDepthStencilBuffer;

        // the camera
        private CameraComponent camera;

        // flag to render in a single pass
        private bool renderInSinglePass;

        #endregion

        #region Public members

        /// <summary>
        /// The cubemap texture.
        /// </summary>
        public TextureCube TextureCube;

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
        /// <param name="mapSize">The size of the cubemap.</param>
        /// <param name="singlePass">A flag stating if the cubemap should be rendered in one or 6 passes.</param>
        /// <param name="cubeMapPosition">The origin of the cubemap</param>
        /// <param name="nearPlane">The near plane.</param>
        /// <param name="farPlane">The far plane.</param>
        public CubeMapRenderer(IServiceRegistry services, RenderPipeline recursivePipeline, int mapSize, bool singlePass, Vector3 cubeMapPosition, float nearPlane, float farPlane) : base(services, recursivePipeline)
        {
            renderInSinglePass = singlePass;
            cubemapSize = mapSize;
            
            // TODO: simplify that - move to load?
            var targetEntity = new Entity() { new TransformationComponent() };
            camera = new CameraComponent()
            {
                AspectRatio = 1,
                FarPlane = farPlane,
                NearPlane = nearPlane,
                VerticalFieldOfView = MathUtil.PiOverTwo,
                Target = targetEntity,
            };
            // attach the camera component to an entity to perform computation of transformation matrices
            var cameraCube = new Entity(cubeMapPosition) { camera };

            // TODO: mip maps?
            TextureCube = TextureCube.New(GraphicsDevice, cubemapSize, 0, PixelFormat.R8G8B8A8_UNorm, TextureFlags.ShaderResource | TextureFlags.RenderTarget);
        }

        #endregion

        #region Public methods

        public override void Load()
        {
            base.Load();

            if (renderInSinglePass)
            {
                cubeMapRenderTarget = TextureCube.ToRenderTarget(ViewType.Full, 0, 0);
                cubeMapDepthStencilBuffer = Texture2D.New(GraphicsDevice, cubemapSize, cubemapSize, PixelFormat.D24_UNorm_S8_UInt, TextureFlags.DepthStencil, 6).ToDepthStencilBuffer(false);
            }
            else
            {
                cubeMapRenderTargetsArray = new RenderTarget[6];
                for (var i = 0; i < 6; ++i)
                    cubeMapRenderTargetsArray[i] = TextureCube.ToRenderTarget(ViewType.Single, i, 0);
                cubeMapDepthStencilBuffer = Texture2D.New(GraphicsDevice, cubemapSize, cubemapSize, PixelFormat.D24_UNorm_S8_UInt, TextureFlags.DepthStencil).ToDepthStencilBuffer(false);
            }

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
            if (renderInSinglePass)
                RenderInSinglePass(context);
            else
                RenderInSixPasses(context);

            // NOTE: this is really slow so it should be avoided
            //for (var i = 0; i < 6; ++i)
            //    Textures2D[i].SetData(GraphicsDevice, TextureCube.GetData<uint>(i));

            GraphicsDevice.GenerateMips(TextureCube);
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Renders the cubemap in 6 passes, one for each face.
        /// </summary>
        /// <param name="context">The render context.</param>
        private void RenderInSixPasses(RenderContext context)
        {
            var cameraPos = camera.Entity.Transformation.Translation;
            camera.Entity.Transformation.UpdateWorldMatrix();
            for (var i = 0; i < 6; ++i)
            {
                camera.Target.Transformation.Translation = cameraPos + targetPositions[i];
                camera.Target.Transformation.UpdateWorldMatrix();
                camera.TargetUp = cameraUps[i];
                ComputeCameraTransformations(context);
                GraphicsDevice.Clear(cubeMapDepthStencilBuffer, DepthStencilClearOptions.DepthBuffer);
                GraphicsDevice.Clear(cubeMapRenderTargetsArray[i], Color.Black);
                GraphicsDevice.SetRenderTargets(cubeMapDepthStencilBuffer, cubeMapRenderTargetsArray[i]);
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
        private void RenderInSinglePass(RenderContext context)
        {
            ComputeAllCameraMatrices(context);

            GraphicsDevice.Clear(cubeMapDepthStencilBuffer, DepthStencilClearOptions.DepthBuffer);
            GraphicsDevice.Clear(cubeMapRenderTarget, Color.Black);
            
            GraphicsDevice.SetRenderTargets(cubeMapDepthStencilBuffer, cubeMapRenderTarget);
            
            base.Render(context);

            GraphicsDevice.SetRenderTarget(GraphicsDevice.DepthStencilBuffer, GraphicsDevice.BackBuffer);

            GraphicsDevice.Parameters.Remove(CameraCubeKeys.CameraViewProjectionMatrices);
            GraphicsDevice.Parameters.Remove(CameraCubeKeys.CameraWorldPosition);
        }

        /// <summary>
        /// Computes the parameters of the camera.
        /// </summary>
        /// <param name="context">The render context.</param>
        private void ComputeCameraTransformations(RenderContext context)
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
        private void ComputeAllCameraMatrices(RenderContext context)
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
