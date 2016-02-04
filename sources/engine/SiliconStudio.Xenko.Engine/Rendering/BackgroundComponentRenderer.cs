// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;

using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Engine.Processors;
using SiliconStudio.Xenko.Graphics;

namespace SiliconStudio.Xenko.Rendering
{
    /// <summary>
    /// This renderer draws a full-screen image as background. 
    /// The ratio or the texture used is preserved. The texture is centered and cropped along X or Y axis depending on the screen ratio.
    /// </summary>
    /// <remarks>This renderer does not write into the depth buffer</remarks>
    public class BackgroundComponentRenderer : EntityComponentRendererBase
    {
        private SpriteBatch spriteBatch;

        private EffectInstance backgroundEffect;

        protected override void InitializeCore()
        {
            base.InitializeCore();

            backgroundEffect = new EffectInstance(new Effect(Context.GraphicsDevice, BackgroundEffect.Bytecode) { Name = "BackgroundEffect" });
            spriteBatch = new SpriteBatch(Context.GraphicsDevice) { VirtualResolution = new Vector3(1)};
        }

        protected override void PrepareCore(RenderDrawContext context, RenderItemCollection opaqueList, RenderItemCollection transparentList)
        {
            var backgroundProcessor = SceneInstance.GetProcessor<BackgroundComponentProcessor>();
            if (backgroundProcessor == null)
                return;

            foreach (var backgroundComponent in backgroundProcessor.Backgrounds)
            {
                // Perform culling on group and accept
                if (!CurrentCullingMask.Contains(backgroundComponent.Entity.Group))
                    continue;

                opaqueList.Add(new RenderItem(this, backgroundComponent, float.NegativeInfinity)); // render background first so that it can replace a clear frame
                return; // draw only one background by group
            }
        }

        protected override void DrawCore(RenderDrawContext context, RenderItemCollection renderItems, int fromIndex, int toIndex)
        {
            var graphicsDevice = context.GraphicsDevice;
            var destination = new RectangleF(0, 0, 1, 1);

            // find the last background to display with valid texture
            BackgroundComponent background = null;
            for (var i = toIndex; i >= fromIndex; --i)
            {
                background = (BackgroundComponent)renderItems[i].DrawContext;
                if (background.Texture != null)
                    break;
            }

            // Abort if not valid background component
            if (background == null || background.Texture == null)
                return;

            var texture = background.Texture;
            var target = CurrentRenderFrame;
            var imageBufferMinRatio = Math.Min(texture.ViewWidth / (float)target.Width, texture.ViewHeight / (float)target.Height);
            var sourceSize = new Vector2(target.Width * imageBufferMinRatio, target.Height * imageBufferMinRatio);
            var source = new RectangleF((texture.ViewWidth - sourceSize.X) / 2, (texture.ViewHeight - sourceSize.Y) / 2, sourceSize.X, sourceSize.Y);

            spriteBatch.Parameters.SetValueSlow(BackgroundEffectKeys.Intensity, background.Intensity);
            spriteBatch.Begin(SpriteSortMode.FrontToBack, graphicsDevice.BlendStates.Opaque, graphicsDevice.SamplerStates.LinearClamp, graphicsDevice.DepthStencilStates.None, null, backgroundEffect);
            spriteBatch.Draw(texture, destination, source, Color.White, 0, Vector2.Zero);
            spriteBatch.End();
        }
    }
}