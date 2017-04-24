// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Rendering.Compositing;

namespace SiliconStudio.Xenko.Rendering.Skyboxes
{
    public struct CubemapSceneRenderer : IDisposable
    {
        private readonly ISceneRendererContext context;
        private readonly ISceneRenderer gameCompositor;

        public readonly CameraComponent Camera;
        public readonly RenderDrawContext DrawContext;

        private Texture renderTarget;
        private Texture depthStencil;

        public CubemapSceneRenderer(ISceneRendererContext context, int textureSize)
        {
            this.context = context;

            Camera = new CameraComponent
            {
                UseCustomProjectionMatrix = true,
                UseCustomViewMatrix = true,
            };

            var slot = new SceneCameraSlot();
            Camera.Slot = slot.ToSlotId();
            context.SceneSystem.GraphicsCompositor.Cameras.Add(slot);

            // Replace graphics compositor (don't want post fx, etc...)
            gameCompositor = context.SceneSystem.GraphicsCompositor.Game;
            context.SceneSystem.GraphicsCompositor.Game = new SceneCameraRenderer { Child = context.SceneSystem.GraphicsCompositor.SingleView, Camera = slot };

            // Setup projection matrix
            Camera.ProjectionMatrix = Matrix.PerspectiveFovRH(MathUtil.DegreesToRadians(90.0f), 1.0f, Camera.NearClipPlane, Camera.FarClipPlane);

            var renderContext = RenderContext.GetShared(context.Services);
            DrawContext = new RenderDrawContext(context.Services, renderContext, context.GraphicsContext);

            // We can't render directly to the texture cube before feature level 10.1, so let's render to a standard render target and copy instead
            renderTarget = Texture.New2D(context.GraphicsDevice, textureSize, textureSize, PixelFormat.R16G16B16A16_Float, TextureFlags.RenderTarget);
            depthStencil = Texture.New2D(context.GraphicsDevice, textureSize, textureSize, PixelFormat.D24_UNorm_S8_UInt, TextureFlags.DepthStencil);
        }

        public void Dispose()
        {
            context.SceneSystem.GraphicsCompositor.Game = gameCompositor;
            context.SceneSystem.GraphicsCompositor.Cameras.RemoveAt(context.SceneSystem.GraphicsCompositor.Cameras.Count - 1);

            renderTarget.Dispose();
            depthStencil.Dispose();
        }

        /// <summary>
        /// Render scene from a given position to a cubemap.
        /// </summary>
        /// <param name="position"></param>
        /// <param name="cubeTexture"></param>
        public void Draw(Vector3 position, Texture cubeTexture)
        {
            DrawContext.CommandList.SetRenderTargetAndViewport(depthStencil, renderTarget);

            for (int face = 0; face < 6; ++face)
            {
                // Place camera
                switch ((CubeMapFace)face)
                {
                    case CubeMapFace.PositiveX:
                        Camera.ViewMatrix = Matrix.LookAtRH(position, position + Vector3.UnitX, Vector3.UnitY);
                        break;
                    case CubeMapFace.NegativeX:
                        Camera.ViewMatrix = Matrix.LookAtRH(position, position - Vector3.UnitX, Vector3.UnitY);
                        break;
                    case CubeMapFace.PositiveY:
                        Camera.ViewMatrix = Matrix.LookAtRH(position, position + Vector3.UnitY, Vector3.UnitZ);
                        break;
                    case CubeMapFace.NegativeY:
                        Camera.ViewMatrix = Matrix.LookAtRH(position, position - Vector3.UnitY, -Vector3.UnitZ);
                        break;
                    case CubeMapFace.PositiveZ:
                        Camera.ViewMatrix = Matrix.LookAtRH(position, position - Vector3.UnitZ, Vector3.UnitY);
                        break;
                    case CubeMapFace.NegativeZ:
                        Camera.ViewMatrix = Matrix.LookAtRH(position, position + Vector3.UnitZ, Vector3.UnitY);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                DrawContext.CommandList.BeginProfile(Color.Red, $"Face {(CubeMapFace)face}");

                // Draw
                context.GameSystems.Draw(context.DrawTime);

                // Copy to texture cube
                DrawContext.CommandList.CopyRegion(DrawContext.CommandList.RenderTarget, 0, null, cubeTexture, face);

                DrawContext.CommandList.EndProfile();
            }
        }

        public static Texture GenerateCubemap(ISceneRendererContext context, Vector3 position, int textureSize)
        {
            using (var cubemapRenderer = new CubemapSceneRenderer(context, textureSize))
            {
                // Create target cube texture
                var cubeTexture = Texture.NewCube(context.GraphicsDevice, textureSize, PixelFormat.R16G16B16A16_Float);

                using (cubemapRenderer.DrawContext.PushRenderTargetsAndRestore())
                {
                    // Render specular probe
                    context.GraphicsContext.CommandList.BeginProfile(Color.Red, "SpecularProbe");

                    cubemapRenderer.Draw(position, cubeTexture);

                    context.GraphicsContext.CommandList.EndProfile();
                }

                return cubeTexture;
            }
        }
    }
}
