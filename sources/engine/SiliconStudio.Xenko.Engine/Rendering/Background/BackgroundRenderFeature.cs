// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Rendering.Skyboxes;
using SiliconStudio.Xenko.Streaming;

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

        public override void Prepare(RenderDrawContext context)
        {
            base.Prepare(context);

            // Register resources usage
            foreach (var renderObject in RenderObjects)
            {
                var renderBackground = (RenderBackground)renderObject;
                Context.StreamingManager?.StreamResources(renderBackground.Texture, StreamingOptions.LoadAtOnce);
            }
        }

        public override void Draw(RenderDrawContext context, RenderView renderView, RenderViewStage renderViewStage, int startIndex, int endIndex)
        {
            for (int index = startIndex; index < endIndex; index++)
            {
                var renderNodeReference = renderViewStage.SortedRenderNodes[index].RenderNode;
                var renderNode = GetRenderNode(renderNodeReference);
                var renderBackground = (RenderBackground)renderNode.RenderObject;

                if (renderBackground.Texture == null)
                    continue;
                    
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
            var textureIsLoading = texture.ViewType == ViewType.Full && texture.FullQualitySize.Width != texture.ViewWidth;
            var textureSize = textureIsLoading ? texture.FullQualitySize: new Size3(texture.ViewWidth, texture.ViewHeight, texture.ViewDepth);
            var imageBufferMinRatio = Math.Min(textureSize.Width / (float)target.ViewWidth, textureSize.Height / (float)target.ViewHeight);
            var sourceSize = new Vector2(target.ViewWidth * imageBufferMinRatio, target.ViewHeight * imageBufferMinRatio);
            var source = new RectangleF((textureSize.Width - sourceSize.X) / 2, (textureSize.Height - sourceSize.Y) / 2, sourceSize.X, sourceSize.Y);
            if (textureIsLoading)
            {
                var verticalRatio = texture.ViewHeight / (float)textureSize.Height;
                var horizontalRatio = texture.ViewWidth / (float)textureSize.Width;
                source.X *= horizontalRatio;
                source.Width *= horizontalRatio;
                source.Y *= verticalRatio;
                source.Height *= verticalRatio;
            }

            backgroundEffect.UpdateEffect(graphicsDevice);
            spriteBatch.Begin(context.GraphicsContext, SpriteSortMode.FrontToBack, BlendStates.Opaque, graphicsDevice.SamplerStates.LinearClamp, DepthStencilStates.DepthRead, null, backgroundEffect);
            spriteBatch.Parameters.Set(BackgroundShaderKeys.Intensity, renderBackground.Intensity);
            spriteBatch.Draw(texture, destination, source, Color.White, 0, Vector2.Zero, layerDepth: -0.5f);
            spriteBatch.End();
        }

        private void DrawCube(RenderDrawContext context, RenderView renderView, RenderBackground renderBackground)
        {
            var graphicsDevice = context.GraphicsDevice;
            var destination = new RectangleF(0, 0, 1, 1);

            var texture = renderBackground.Texture;
            
            skyboxBackgroundEffect.UpdateEffect(graphicsDevice);
            spriteBatch.Begin(context.GraphicsContext, SpriteSortMode.FrontToBack, BlendStates.Opaque, graphicsDevice.SamplerStates.LinearClamp, DepthStencilStates.DepthRead, null, skyboxBackgroundEffect);
            spriteBatch.Parameters.Set(SkyboxShaderKeys.Intensity, renderBackground.Intensity);
            spriteBatch.Parameters.Set(SkyboxShaderKeys.ViewInverse, Matrix.Invert(renderView.View));
            spriteBatch.Parameters.Set(SkyboxShaderKeys.ProjectionInverse, Matrix.Invert(renderView.Projection));
            spriteBatch.Parameters.Set(SkyboxShaderKeys.SkyMatrix, Matrix.Invert(Matrix.RotationQuaternion(renderBackground.Rotation)));
            spriteBatch.Parameters.Set(SkyboxShaderKeys.CubeMap, renderBackground.Texture);
            spriteBatch.Draw(texture, destination, null, Color.White, 0, Vector2.Zero, layerDepth: -0.5f);
            spriteBatch.End();
        }
    }
}
