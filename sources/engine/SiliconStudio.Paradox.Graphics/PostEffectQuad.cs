// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;

using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.Effects.Modules;

namespace SiliconStudio.Paradox.Graphics
{
    public class PostEffectQuad : ComponentBase
    {
        private readonly Effect effect;
        private readonly SharedData sharedData;
        private const int QuadCount = 3;

        /// <summary>
        /// Initializes a new instance of the <see cref="PostEffectQuad" /> class.
        /// </summary>
        /// <param name="graphicsDevice">The graphics device.</param>
        /// <param name="effect">The effect that will be used.</param>
        public PostEffectQuad(GraphicsDevice graphicsDevice, Effect effect)
        {
            GraphicsDevice = graphicsDevice;
            this.effect = effect;
            sharedData = GraphicsDevice.GetOrCreateSharedData(GraphicsDeviceSharedDataType.PerDevice, "PostEffectQuad::VertexBuffer", () => new SharedData(GraphicsDevice, this.effect.InputSignature));
        }

        /// <summary>
        /// Gets the graphics device.
        /// </summary>
        /// <value>The graphics device.</value>
        public GraphicsDevice GraphicsDevice { get; private set; }

        /// <summary>
        /// Draws a quad. The effect must have been applied before calling this method.
        /// </summary>
        public void Draw()
        {
            GraphicsDevice.SetVertexArrayObject(sharedData.VertexBuffer);
            GraphicsDevice.Draw(PrimitiveType.TriangleList, QuadCount);
            GraphicsDevice.SetVertexArrayObject(null);
        }

        /// <summary>
        /// Draws a quad with a texture. This Draw method is using a simple pixel shader that is sampling the texture.
        /// </summary>
        /// <param name="texture">The texture to draw.</param>
        /// <exception cref="System.ArgumentException">Expecting a Texture2D;texture</exception>
        public void Draw(Texture texture)
        {
            var texture2D = texture as Texture2D;
            if (texture2D == null) throw new ArgumentException("Expecting a Texture2D", "texture");

            // Make sure that we are using our vertex shader
            effect.Parameters.Set(TexturingKeys.Texture0, texture as Texture2D);
            effect.Apply();
            Draw();

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
            public readonly VertexArrayObject VertexBuffer;

            private static readonly VertexPosition2[] QuadsVertices = new[]
            {
                new VertexPosition2(new Vector2(-1, 1)),
                new VertexPosition2(new Vector2(3, 1)),
                new VertexPosition2(new Vector2(-1, -3))
            };

            public SharedData(GraphicsDevice device, EffectInputSignature defaultSignature)
            {
                var vertexBuffer = Buffer.Vertex.New(device, QuadsVertices).DisposeBy(this);

                // Register reload
                vertexBuffer.Reload = (graphicsResource) => ((Buffer)graphicsResource).Recreate(QuadsVertices);

                VertexBuffer = VertexArrayObject.New(device, defaultSignature, new VertexBufferBinding(vertexBuffer, VertexPosition2.Layout, QuadsVertices.Length, VertexPosition2.Size)).DisposeBy(this);
            }
        }
    }
}
