// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;

using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.Graphics;

using Buffer = SiliconStudio.Paradox.Graphics.Buffer;

namespace SiliconStudio.Paradox.Effects.Images.Cubemap
{
    public class CubeMapEffect : ImageEffectShader
    {
        private Buffer<Vector3> vertexBuffer;
        private VertexArrayObject vertexArrayObject;

        private readonly Texture[] cubeTextureViews = new Texture[6];

        public CubeMapEffect(ImageEffectContext context, string effectName)
            : base(context, effectName)
        {
        }

        protected override void SetRenderTargets()
        {
            // Gets and checks the output texture
            var outputTexture = GetSafeOutput(0);
            if (outputTexture.Dimension != TextureDimension.TextureCube)
                throw new InvalidOperationException("Only textures of type 'TextureCube' are valid as output of 'LambertianPrefiltering' effect.");

            // create the views on the texture cube
            for (int i = 0; i < cubeTextureViews.Length; i++)
                cubeTextureViews[i] = outputTexture.ToTextureView(ViewType.Single, i, 0);

            // override the outputs with the views
            SetOutput(cubeTextureViews);

            base.SetRenderTargets();
        }

        protected override void UpdateParameters()
        {
            base.UpdateParameters();

            // Gets and checks the input texture
            var inputTexture = GetSafeInput(0);
            if (inputTexture.Dimension != TextureDimension.TextureCube)
                throw new InvalidOperationException("Only textures of type 'TextureCube' are valid as input of 'LambertianPrefiltering' effect.");

            Parameters.Set(TexturingKeys.TextureCube0, inputTexture);
        }

        protected override void DrawCore()
        {
            UpdateEffect();

            if (vertexArrayObject == null)
                CreateVertexArrayObject();

            GraphicsDevice.SetBlendState(GraphicsDevice.BlendStates.Opaque);
            GraphicsDevice.SetRasterizerState(GraphicsDevice.RasterizerStates.CullNone);
            GraphicsDevice.SetDepthStencilState(GraphicsDevice.DepthStencilStates.None);
            Parameters.Set(TexturingKeys.Sampler, GraphicsDevice.SamplerStates.PointClamp);

            // Apply the effect
            EffectInstance.Effect.Apply(GraphicsDevice, ParameterCollections, false);

            // Draw the points
            GraphicsDevice.SetVertexArrayObject(vertexArrayObject);
            GraphicsDevice.Draw(PrimitiveType.TriangleStrip, 4);
            GraphicsDevice.SetVertexArrayObject(null);

            // Un-apply the effect
            EffectInstance.Effect.UnbindResources(GraphicsDevice);
        }

        private void CreateVertexArrayObject()
        {
            // calculate the vertices values
            var vertices = new[]
            {
                new Vector3( 1, -1, 1), 
                new Vector3( 1,  1, 1), 
                new Vector3(-1, -1, 1),
                new Vector3(-1,  1, 1)
            };

            // create the buffers
            vertexBuffer = Buffer.Vertex.New(GraphicsDevice, vertices);
            var vertexLayout = new VertexDeclaration(VertexElement.Position<Vector3>());
            var vertexBufferBinding = new VertexBufferBinding(vertexBuffer, vertexLayout, vertices.Length);
            vertexArrayObject = VertexArrayObject.New(GraphicsDevice, EffectInstance.Effect.InputSignature, vertexBufferBinding);
        }

        protected override void PostDrawCore()
        {
            // free the views on the texture
            for (int i = 0; i < cubeTextureViews.Length; i++)
            {
                if (cubeTextureViews[i] != null)
                {
                    cubeTextureViews[i].Dispose();
                    cubeTextureViews[i] = null;
                }
            }

            base.PostDrawCore();
        }

        protected override void Destroy()
        {
            if (vertexArrayObject != null)
            {
                vertexArrayObject.Dispose();
                vertexArrayObject = null;
            }

            if (vertexBuffer != null)
            {
                vertexBuffer.Dispose();
                vertexBuffer = null;
            }

            base.Destroy();
        }
    }
}