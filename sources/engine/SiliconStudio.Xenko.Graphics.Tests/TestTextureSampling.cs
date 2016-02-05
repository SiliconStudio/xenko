// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System.Threading.Tasks;
using NUnit.Framework;

using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Rendering;
using SiliconStudio.Xenko.Games;

namespace SiliconStudio.Xenko.Graphics.Tests
{
    [TestFixture]
    public class TestTextureSampling : GraphicTestGameBase
    {
        struct Vertex
        {
            public Vector3 Position;
            public Vector2 TexCoords;
        };

        private struct DrawOptions
        {
            public Matrix Transform;

            public SamplerState Sampler;
        };

        private EffectInstance simpleEffect;

        // TODO GRAPHICS REFACTOR
        //private VertexArrayObject vao;

        private DrawOptions[] myDraws;

        public TestTextureSampling()
        {
            CurrentVersion = 2;
        }

        protected override void RegisterTests()
        {
            base.RegisterTests();

            FrameGameSystem.Draw(DrawTextureSampling).TakeScreenshot();
        }

        protected override async Task LoadContent()
        {
            await base.LoadContent();

            var vertices = new Vertex[4];
            vertices[0] = new Vertex { Position = new Vector3(-1, -1, 0.5f), TexCoords = new Vector2(0, 0) };
            vertices[1] = new Vertex { Position = new Vector3(-1, 1, 0.5f), TexCoords = new Vector2(3, 0) };
            vertices[2] = new Vertex { Position = new Vector3(1, 1, 0.5f), TexCoords = new Vector2(3, 3) };
            vertices[3] = new Vertex { Position = new Vector3(1, -1, 0.5f), TexCoords = new Vector2(0, 3) };

            var indices = new short[] { 0, 1, 2, 0, 2, 3 };

            var vertexBuffer = Buffer.Vertex.New(GraphicsDevice, vertices, GraphicsResourceUsage.Default);
            var indexBuffer = Buffer.Index.New(GraphicsDevice, indices, GraphicsResourceUsage.Default);
            var meshDraw = new MeshDraw
            {
                DrawCount = 4,
                PrimitiveType = PrimitiveType.TriangleList,
                VertexBuffers = new[]
                {
                    new VertexBufferBinding(vertexBuffer,
                        new VertexDeclaration(VertexElement.Position<Vector3>(),
                            VertexElement.TextureCoordinate<Vector2>()),
                        4)
                },
                IndexBuffer = new IndexBufferBinding(indexBuffer, false, indices.Length),
            };

            var mesh = new Mesh
            {
                Draw = meshDraw,
            };

            simpleEffect = new EffectInstance(new Effect(GraphicsDevice, SpriteEffect.Bytecode));
            simpleEffect.Parameters.SetResourceSlow(TexturingKeys.Texture0, UVTexture);

            // TODO GRAPHICS REFACTOR
            //vao = VertexArrayObject.New(GraphicsDevice, mesh.Draw.IndexBuffer, mesh.Draw.VertexBuffers);

            myDraws = new DrawOptions[3];
            myDraws[0] = new DrawOptions { Sampler = GraphicsDevice.SamplerStates.LinearClamp, Transform = Matrix.Multiply(Matrix.Scaling(0.4f), Matrix.Translation(-0.5f, 0.5f, 0f)) };
            myDraws[1] = new DrawOptions { Sampler = GraphicsDevice.SamplerStates.LinearWrap, Transform = Matrix.Multiply(Matrix.Scaling(0.4f), Matrix.Translation(0.5f, 0.5f, 0f)) };
            myDraws[2] = new DrawOptions { Sampler = SamplerState.New(GraphicsDevice, new SamplerStateDescription(TextureFilter.Linear, TextureAddressMode.Mirror)), Transform = Matrix.Multiply(Matrix.Scaling(0.4f), Matrix.Translation(0.5f, -0.5f, 0f)) };
            //var borderDescription = new SamplerStateDescription(TextureFilter.Linear, TextureAddressMode.Border) { BorderColor = Color.Purple };
            //var border = SamplerState.New(GraphicsDevice, borderDescription);
            //myDraws[3] = new DrawOptions { Sampler = border, Transform = Matrix.Multiply(Matrix.Scale(0.3f), Matrix.Translation(-0.5f, -0.5f, 0f)) };
        }

        protected override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);

            if(!ScreenShotAutomationEnabled)
                DrawTextureSampling();
        }

        private void DrawTextureSampling()
        {
            // Clears the screen 
            GraphicsDevice.Clear(GraphicsDevice.BackBuffer, Color.LightBlue);
            GraphicsDevice.Clear(GraphicsDevice.DepthStencilBuffer, DepthStencilClearOptions.DepthBuffer | DepthStencilClearOptions.Stencil);
            GraphicsDevice.SetDepthAndRenderTarget(GraphicsDevice.DepthStencilBuffer, GraphicsDevice.BackBuffer);

            // TODO GRAPHICS REFACTOR
            //GraphicsDevice.SetRasterizerState(GraphicsDevice.RasterizerStates.CullNone);

            // TODO GRAPHICS REFACTOR
            //GraphicsDevice.SetVertexArrayObject(vao);

            for (var i = 0; i < myDraws.Length; ++i)
            {
                simpleEffect.Parameters.SetResourceSlow(TexturingKeys.Sampler, myDraws[i].Sampler);
                simpleEffect.Parameters.SetValueSlow(SpriteBaseKeys.MatrixTransform, myDraws[i].Transform);
                simpleEffect.Apply(GraphicsDevice);
                GraphicsDevice.DrawIndexed(PrimitiveType.TriangleList, 6);
            }

            // TODO GRAPHICS REFACTOR
            //GraphicsDevice.SetVertexArrayObject(null);
        }

        public static void Main()
        {
            using (var game = new TestTextureSampling())
                game.Run();
        }

        /// <summary>
        /// Run the test
        /// </summary>
        [Test]
        public void RunTestTextureSampling()
        {
            RunGameTest(new TestTextureSampling());
        }
    }
}
