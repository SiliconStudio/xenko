// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.Effects.Modules.Processors;
using SiliconStudio.Paradox.EntityModel;
using SiliconStudio.Paradox.Graphics;

namespace SiliconStudio.Paradox.Effects.Modules.Renderers
{
    public class LightingIBLRenderer : Renderer
    {
        #region Private members

        private RenderTarget IBLRenderTarget;

        private DepthStencilBuffer inputDepthStencilBuffer;

        private Texture depthBufferTexture;

        private GeometricPrimitive cubemapMesh;

        private Effect IBLEffect;

        private BlendState IBLBlendState;

        private DepthStencilState IBLDepthStencilState;

        private ParameterCollection parameters;

        #endregion

        #region Properties

        /// <summary>
        /// The texture the lighting will be rendered into.
        /// </summary>
        public Texture IBLTexture { get; private set; }

        #endregion

        #region Constructor

        public LightingIBLRenderer(IServiceRegistry services, DepthStencilBuffer depthStencilBuffer) : base(services)
        {
            inputDepthStencilBuffer = depthStencilBuffer;
        }

        #endregion

        #region Public methods

        /// <inheritdoc/>
        public override void Load()
        {
            // Create necessary objects
            IBLTexture = Texture2D.New(GraphicsDevice, inputDepthStencilBuffer.Description.Width, inputDepthStencilBuffer.Description.Height, PixelFormat.R16G16B16A16_Float, TextureFlags.ShaderResource | TextureFlags.RenderTarget);
            IBLRenderTarget = IBLTexture.ToRenderTarget();

            cubemapMesh = GeometricPrimitive.Sphere.New(GraphicsDevice);

            var blendStateDescr = new BlendStateDescription()
            {
                RenderTargets = new[]
                {
                    new BlendStateRenderTargetDescription()
                    {
                        BlendEnable = true,
                        ColorSourceBlend = Blend.SourceAlpha,
                        ColorDestinationBlend = Blend.One,
                        ColorBlendFunction = BlendFunction.Add,
                        AlphaSourceBlend = Blend.One,
                        AlphaDestinationBlend = Blend.One,
                        AlphaBlendFunction = BlendFunction.Add,
                        ColorWriteChannels = ColorWriteChannels.All
                    }
                }
            };
            IBLBlendState = BlendState.New(GraphicsDevice, blendStateDescr);

            // depth state to test z-fail of backfaces
            IBLDepthStencilState = DepthStencilState.New(GraphicsDevice, new DepthStencilStateDescription(true, false)
                {
                    StencilEnable = false,
                    DepthBufferFunction = CompareFunction.GreaterEqual,
                });

            // effect
            IBLEffect = EffectSystem.LoadEffect("CubemapIBL");

            // copy of the depth buffer
            depthBufferTexture = Texture2D.New(GraphicsDevice, inputDepthStencilBuffer.Description.Width, inputDepthStencilBuffer.Description.Height, inputDepthStencilBuffer.Description.Format, TextureFlags.DepthStencil | TextureFlags.ShaderResource);

            parameters = new ParameterCollection();
            parameters.Set(RenderTargetKeys.DepthStencilSource, depthBufferTexture);

            // Add to pipeline
            Pass.StartPass += RenderIBL;
        }

        /// <inheritdoc/>
        public override void Unload()
        {
            // Remove from pipeline
            Pass.StartPass -= RenderIBL;

            parameters.Clear();

            depthBufferTexture.Dispose();
            IBLEffect.Dispose();
            IBLDepthStencilState.Dispose();
            IBLBlendState.Dispose();
            cubemapMesh.Dispose();
            IBLRenderTarget.Dispose();
            IBLTexture.Dispose();
        }

        #endregion

        #region Private methods

        private void RenderIBL(RenderContext context)
        {
            var entitySystem = Services.GetServiceAs<EntitySystem>();
            var cubemapSourceProcessor = entitySystem.GetProcessor<CubemapSourceProcessor>();
            if (cubemapSourceProcessor == null)
                return;

            // clear render target
            GraphicsDevice.Clear(IBLRenderTarget, new Color4(0, 0, 0, 0));

            // if no cubemap, exit
            if (cubemapSourceProcessor.Cubemaps.Count <= 0)
                return;

            // copy depth buffer
            GraphicsDevice.Copy(inputDepthStencilBuffer.Texture, depthBufferTexture);

            // set render target
            GraphicsDevice.SetRenderTarget(GraphicsDevice.DepthStencilBuffer, IBLRenderTarget);
            
            // set blend state
            GraphicsDevice.SetBlendState(IBLBlendState);

            // set depth state
            GraphicsDevice.SetDepthStencilState(IBLDepthStencilState);

            // set culling
            GraphicsDevice.SetRasterizerState(GraphicsDevice.RasterizerStates.CullFront);

            foreach (var cubemap in cubemapSourceProcessor.Cubemaps)
            {
                // set world matrix matrices
                // TODO: rotation of cubemap & sphere mesh
                parameters.Set(TransformationKeys.World, ComputeTransformationMatrix(cubemap.Value.InfluenceRadius, cubemap.Key.Transformation.Translation));
                parameters.Set(CubemapIBLKeys.CubemapRadius, cubemap.Value.InfluenceRadius);
                parameters.Set(CubemapIBLKeys.Cubemap, cubemap.Value.Texture);
                parameters.Set(CubemapIBLKeys.CubemapPosition, cubemap.Key.Transformation.Translation);
                parameters.Set(CubemapIBLWithRoughnessKeys.CubemapRoughness, 0.5f);

                // apply effect
                IBLEffect.Apply(parameters, context.CurrentPass.Parameters);

                // render cubemap
                cubemapMesh.Draw(GraphicsDevice);
            }
        }

        #endregion

        #region Helpers

        private static Matrix ComputeTransformationMatrix(float size, Vector3 position)
        {
            // x2 because the size is a radius
            return Matrix.Scaling(2 * size) * Matrix.Translation(position);
        }

        #endregion
    }
}