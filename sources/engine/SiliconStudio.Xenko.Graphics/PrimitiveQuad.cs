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
        /// <summary>
        /// The pipeline state.
        /// </summary>
        private readonly MutablePipelineState pipelineState = new MutablePipelineState();

        private readonly EffectInstance simpleEffect;
        private readonly SharedData sharedData;
        private const int QuadCount = 3;

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
            sharedData = GraphicsDevice.GetOrCreateSharedData(GraphicsDeviceSharedDataType.PerDevice, "PrimitiveQuad::VertexBuffer", d => new SharedData(GraphicsDevice));

            simpleEffect = new EffectInstance(new Effect(GraphicsDevice, SpriteEffect.Bytecode));
            simpleEffect.UpdateEffect(graphicsDevice);
            simpleEffect.Parameters.Set(SpriteBaseKeys.MatrixTransform, Matrix.Identity);

            pipelineState.State.SetDefaults();
            pipelineState.State.InputElements = VertexDeclaration.CreateInputElements();
            pipelineState.State.PrimitiveType = PrimitiveType;
            pipelineState.State.RootSignature = simpleEffect.RootSignature;
            pipelineState.State.EffectBytecode = simpleEffect.Effect.Bytecode;
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
        public NextGenParameterCollection Parameters => simpleEffect.Parameters;

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
        public void Draw(GraphicsContext graphicsContext, Texture texture, bool applyEffectStates = false)
        {
            Draw(graphicsContext, texture, null, Color.White, applyEffectStates);
        }

        /// <summary>
        /// Draws a quad with a texture. This Draw method is using a simple pixel shader that is sampling the texture.
        /// </summary>
        /// <param name="texture">The texture to draw.</param>
        /// <param name="samplerState">State of the sampler. If null, default sampler is <see cref="SamplerStateFactory.LinearClamp" />.</param>
        /// <param name="color">The color.</param>
        /// <param name="applyEffectStates">The flag to apply effect states.</param>
        /// <exception cref="System.ArgumentException">Expecting a Texture;texture</exception>
        public void Draw(GraphicsContext graphicsContext, Texture texture, SamplerState samplerState, Color4 color, bool applyEffectStates = false)
        {
            // Make sure that we are using our vertex shader
            simpleEffect.Parameters.Set(SpriteEffectKeys.Color, color);
            simpleEffect.Parameters.Set(TexturingKeys.Texture0, texture);
            simpleEffect.Parameters.Set(TexturingKeys.Sampler, samplerState ?? GraphicsDevice.SamplerStates.LinearClamp);
            simpleEffect.Apply(graphicsContext);

            pipelineState.State.Output.CaptureState(graphicsContext.CommandList);
            pipelineState.Update(GraphicsDevice);
            graphicsContext.CommandList.SetPipelineState(pipelineState.CurrentState);

            Draw(graphicsContext.CommandList);

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