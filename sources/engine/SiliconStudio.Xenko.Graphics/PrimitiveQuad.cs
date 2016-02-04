// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Rendering;

namespace SiliconStudio.Xenko.Graphics
{
    /// <summary>
    /// Primitive quad use to draw an effect on a quad (fullscreen by default). This is directly accessible from the <see cref="GraphicsDevice.DrawQuad"/> method.
    /// </summary>
    public class PrimitiveQuad : ComponentBase
    {
        private readonly Effect simpleEffect;
        private readonly SharedData sharedData;
        private const int QuadCount = 3;

        private readonly ParameterCollection parameters;

        public static readonly VertexDeclaration VertexDeclaration = VertexPositionNormalTexture.Layout;
        public static readonly PrimitiveType PrimitiveType = PrimitiveType.TriangleList;

        /// <summary>
        /// Initializes a new instance of the <see cref="PrimitiveQuad" /> class.
        /// </summary>
        /// <param name="graphicsDevice">The graphics device.</param>
        /// <param name="effect">The effect.</param>
        public PrimitiveQuad(GraphicsDevice graphicsDevice)
        {
            GraphicsDevice = graphicsDevice;
            parameters = new ParameterCollection();
            parameters.Set(SpriteBaseKeys.MatrixTransform, Matrix.Identity);
            sharedData = GraphicsDevice.GetOrCreateSharedData(GraphicsDeviceSharedDataType.PerDevice, "PrimitiveQuad::VertexBuffer", d => new SharedData(GraphicsDevice));
        }

        /// <summary>
        /// Gets the graphics device.
        /// </summary>
        /// <value>The graphics device.</value>
        public GraphicsDevice GraphicsDevice { get; private set; }

        /// <summary>
        /// Gets the parameters used.
        /// </summary>
        /// <value>The parameters.</value>
        public ParameterCollection Parameters
        {
            get
            {
                return parameters;
            }
        }

        /// <summary>
        /// Draws a quad. The effect must have been applied before calling this method with pixel shader having the signature float2:TEXCOORD.
        /// </summary>
        /// <param name="texture"></param>
        public void Draw(CommandList commandList)
        {
            //GraphicsDevice.SetVertexArrayObject(sharedData.VertexBuffer);
            commandList.SetVertexBuffer(0, sharedData.VertexBuffer.Buffer, sharedData.VertexBuffer.Offset, sharedData.VertexBuffer.Stride);
            commandList.Draw(QuadCount);
            //GraphicsDevice.SetVertexArrayObject(null);
        }

        /// <summary>
        /// Draws a quad with a texture. This Draw method is using the current effect bound to this instance.
        /// </summary>
        /// <param name="texture">The texture.</param>
        /// <param name="applyEffectStates">The flag to apply effect states.</param>
        public void Draw(CommandList commandList, Texture texture, bool applyEffectStates = false)
        {
            Draw(commandList, texture, null, Color.White, applyEffectStates);
        }

        /// <summary>
        /// Draws a quad with a texture. This Draw method is using a simple pixel shader that is sampling the texture.
        /// </summary>
        /// <param name="texture">The texture to draw.</param>
        /// <param name="samplerState">State of the sampler. If null, default sampler is <see cref="SamplerStateFactory.LinearClamp" />.</param>
        /// <param name="color">The color.</param>
        /// <param name="applyEffectStates">The flag to apply effect states.</param>
        /// <exception cref="System.ArgumentException">Expecting a Texture;texture</exception>
        public void Draw(CommandList commandList, Texture texture, SamplerState samplerState, Color4 color, bool applyEffectStates = false)
        {
            // Make sure that we are using our vertex shader
            parameters.Set(SpriteEffectKeys.Color, color);
            parameters.Set(TexturingKeys.Texture0, texture);
            parameters.Set(TexturingKeys.Sampler, samplerState ?? GraphicsDevice.SamplerStates.LinearClamp);
            //simpleEffect.Apply(GraphicsDevice, parameterCollectionGroup, applyEffectStates);
            throw new InvalidOperationException();
            Draw(commandList);

            // TODO ADD QUICK UNBIND FOR SRV
            //GraphicsDevice.Context.PixelShader.SetShaderResource(0, null);
        }

        /// <summary>
        /// Internal structure used to store VertexBuffer and VertexInputLayout.
        /// </summary>
        private class SharedData : ComponentBase
        {
            /// <summary>
            /// The vertex buffer
            /// </summary>
            public readonly VertexBufferBinding VertexBuffer;
            
            private static readonly VertexPositionNormalTexture[] QuadsVertices =
            {
                new VertexPositionNormalTexture(new Vector3(-1, 1, 0), new Vector3(0, 0, 1), new Vector2(0, 0)),
                new VertexPositionNormalTexture(new Vector3( 3, 1, 0), new Vector3(0, 0, 1), new Vector2(2, 0)),
                new VertexPositionNormalTexture(new Vector3(-1,-3, 0), new Vector3(0, 0, 1), new Vector2(0, 2)),
            };

            public SharedData(GraphicsDevice device)
            {
                var vertexBuffer = Buffer.Vertex.New(device, QuadsVertices).DisposeBy(this);
                
                // Register reload
                vertexBuffer.Reload = (graphicsResource) => ((Buffer)graphicsResource).Recreate(QuadsVertices);

                VertexBuffer = new VertexBufferBinding(vertexBuffer, VertexDeclaration, QuadsVertices.Length, VertexPositionNormalTexture.Size);
            }
        }
    }
}