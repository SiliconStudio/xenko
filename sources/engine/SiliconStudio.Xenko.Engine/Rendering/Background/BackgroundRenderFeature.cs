// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Linq;
using SiliconStudio.Core;
using SiliconStudio.Core.Collections;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Rendering.Skyboxes;
using SiliconStudio.Xenko.Shaders;

namespace SiliconStudio.Xenko.Rendering.Background
{
    public class BackgroundRenderFeature : RootRenderFeature
    {
        private SpriteBatch spriteBatch;
        private DynamicEffectInstance backgroundEffect;
        private DynamicEffectInstance skyboxBackgroundEffect;

        public override Type SupportedRenderObjectType => typeof(RenderBackground);

        public BackgroundRenderFeature()
        {
            // Background should render after most objects (to take advantage of early z depth test)
            SortKey = 192;
        }

        public override void Draw(RenderDrawContext context, RenderView renderView, RenderViewStage renderViewStage, int startIndex, int endIndex)
        {
            for (int index = startIndex; index < endIndex; index++)
            {
                var renderNodeReference = renderViewStage.SortedRenderNodes[index].RenderNode;
                var renderNode = GetRenderNode(renderNodeReference);
                var renderBackground = (RenderBackground)renderNode.RenderObject;

                if (renderBackground.Texture.Dimension == TextureDimension.Texture2D)
                {
                    Draw2D(context, renderBackground);
                }
                else if (renderBackground.Texture.Dimension == TextureDimension.TextureCube)
                {
                    DrawCube(context, renderView, renderBackground);
                }
            }
        }

        protected override void InitializeCore()
        {
            base.InitializeCore();

            backgroundEffect = new DynamicEffectInstance("BackgroundShader");
            skyboxBackgroundEffect = new DynamicEffectInstance("SkyboxShader");
            backgroundEffect.Initialize(Context.Services);
            skyboxBackgroundEffect.Initialize(Context.Services);
            spriteBatch = new SpriteBatch(RenderSystem.GraphicsDevice) { VirtualResolution = new Vector3(1) };
        }

        private void Draw2D(RenderDrawContext context, RenderBackground renderBackground)
        {
            var target = context.CommandList.RenderTarget;
            var graphicsDevice = context.GraphicsDevice;
            var destination = new RectangleF(0, 0, 1, 1);

            var texture = renderBackground.Texture;

            var imageBufferMinRatio = Math.Min(texture.ViewWidth / (float)target.ViewWidth, texture.ViewHeight / (float)target.ViewHeight);
            var sourceSize = new Vector2(target.ViewWidth * imageBufferMinRatio, target.ViewHeight * imageBufferMinRatio);
            var source = new RectangleF((texture.ViewWidth - sourceSize.X) / 2, (texture.ViewHeight - sourceSize.Y) / 2, sourceSize.X, sourceSize.Y);

            backgroundEffect.UpdateEffect(graphicsDevice);
            spriteBatch.Begin(context.GraphicsContext, SpriteSortMode.FrontToBack, BlendStates.Opaque, graphicsDevice.SamplerStates.LinearClamp, DepthStencilStates.DepthRead, null, backgroundEffect);
            spriteBatch.Parameters.Set(BackgroundShaderKeys.Intensity, renderBackground.Intensity);
            spriteBatch.Draw(texture, destination, source, Color.White, 0, Vector2.Zero, layerDepth: -0.5f);
            spriteBatch.End();
        }

        private void DrawCube(RenderDrawContext context, RenderView renderView, RenderBackground renderBackground)
        {
            var target = context.CommandList.RenderTarget;
            var graphicsDevice = context.GraphicsDevice;
            var destination = new RectangleF(0, 0, 1, 1);

            var texture = renderBackground.Texture;

            var imageBufferMinRatio = Math.Min(texture.ViewWidth / (float)target.ViewWidth, texture.ViewHeight / (float)target.ViewHeight);
            var sourceSize = new Vector2(target.ViewWidth * imageBufferMinRatio, target.ViewHeight * imageBufferMinRatio);
            var source = new RectangleF((texture.ViewWidth - sourceSize.X) / 2, (texture.ViewHeight - sourceSize.Y) / 2, sourceSize.X, sourceSize.Y);

            skyboxBackgroundEffect.UpdateEffect(graphicsDevice);
            spriteBatch.Begin(context.GraphicsContext, SpriteSortMode.FrontToBack, BlendStates.Opaque, graphicsDevice.SamplerStates.LinearClamp, DepthStencilStates.DepthRead, null, skyboxBackgroundEffect);
            spriteBatch.Parameters.Set(SkyboxShaderKeys.Intensity, renderBackground.Intensity);
            spriteBatch.Parameters.Set(SkyboxShaderKeys.ViewInverse, Matrix.Invert(renderView.View));
            spriteBatch.Parameters.Set(SkyboxShaderKeys.ProjectionInverse, Matrix.Invert(renderView.Projection));
            spriteBatch.Parameters.Set(SkyboxShaderKeys.SkyMatrix, Matrix.Invert(Matrix.RotationQuaternion(renderBackground.Rotation)));
            spriteBatch.Parameters.Set(SkyboxShaderKeys.CubeMap, renderBackground.Texture);
            spriteBatch.Draw(texture, destination, source, Color.White, 0, Vector2.Zero, layerDepth: -0.5f);
            spriteBatch.End();
        }
    }
}