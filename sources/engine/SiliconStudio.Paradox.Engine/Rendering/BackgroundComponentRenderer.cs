// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;

using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.Rendering;
using SiliconStudio.Paradox.Engine;
using SiliconStudio.Paradox.Engine.Processors;
using SiliconStudio.Paradox.Graphics;

namespace SiliconStudio.Paradox.Rendering
{
    /// <summary>
    /// This renderer draws a full-screen image as background. 
    /// The ratio or the texture used is preserved. The texture is centered and cropped along X or Y axis depending on the screen ratio.
    /// </summary>
    /// <remarks>This renderer does not write into the depth buffer</remarks>
    public class BackgroundComponentRenderer : EntityComponentRendererBase
    {
        private SpriteBatch spriteBatch;
        
        protected override void InitializeCore()
        {
            base.InitializeCore();

            spriteBatch = new SpriteBatch(Context.GraphicsDevice) { VirtualResolution = new Vector3(1)};
        }

        protected override void PrepareCore(RenderContext context, RenderItemCollection opaqueList, RenderItemCollection transparentList)
        {
            var backgroundProcessor = SceneInstance.GetProcessor<BackgroundComponentProcessor>();
            if (backgroundProcessor == null)
                return;

            foreach (var backgroundComponent in backgroundProcessor.Backgrounds)
            {
                // Perform culling on group and accept
                if (!CurrentCullingMask.Contains(backgroundComponent.Entity.Group))
                    continue;

                opaqueList.Add(new RenderItem(this, backgroundComponent, 0.0f)); // render background first so that it can replace a clear frame
                return; // draw only one background by group
            }
        }

        protected override void DrawCore(RenderContext context, RenderItemCollection renderItems, int fromIndex, int toIndex)
        {
            var graphicsDevice = context.GraphicsDevice;
            var destination = new RectangleF(0, 0, 1, 1);

            spriteBatch.Begin(SpriteSortMode.FrontToBack, graphicsDevice.BlendStates.Opaque, graphicsDevice.SamplerStates.LinearClamp, graphicsDevice.DepthStencilStates.None);

            for(var i = fromIndex; i <= toIndex; ++i)
            {
                var background = (BackgroundComponent)renderItems[i].DrawContext;
                var texture = background.Texture;
                if (texture == null)
                    continue;
                
                var target = CurrentRenderFrame;

                var imageBufferMinRatio = Math.Min(texture.ViewWidth / (float)target.Width, texture.ViewHeight / (float)target.Height);
                var sourceSize = new Vector2(target.Width * imageBufferMinRatio, target.Height * imageBufferMinRatio);
                var source = new RectangleF((texture.ViewWidth - sourceSize.X) / 2, (texture.ViewHeight - sourceSize.Y) / 2, sourceSize.X, sourceSize.Y);

                spriteBatch.Draw(texture, destination, source, Color.White, 0, Vector2.Zero);
            }

            spriteBatch.End();
        }
    }
}