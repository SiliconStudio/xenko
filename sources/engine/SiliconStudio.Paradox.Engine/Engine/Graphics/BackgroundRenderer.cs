// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;

using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Core.Serialization.Assets;
using SiliconStudio.Paradox.Engine.Graphics;
using SiliconStudio.Paradox.Graphics;

namespace SiliconStudio.Paradox.Effects
{
    /// <summary>
    /// This renderer draws a full-screen image as background. 
    /// The ratio or the texture used is preserved. The texture is centered and cropped along X or Y axis depending on the screen ratio.
    /// </summary>
    /// <remarks>This renderer does not write into the depth buffer</remarks>
    public class BackgroundRenderer : EntityComponentRendererBase
    {
        private readonly SpriteBatch spriteBatch;


        /// <summary>
        /// Initializes a new instance of the <see cref="BackgroundRenderer"/> with null texture.
        /// </summary>
        /// <param name="services">The services.</param>
        public BackgroundRenderer()
        {
        }

        ///// <summary>
        ///// Initializes a new instance of the <see cref="BackgroundRenderer"/> using the provided file as background texture.
        ///// </summary>
        ///// <param name="services">The services.</param>
        ///// <param name="backgroundTexturePath">The path to the background texture to use</param>
        //public BackgroundRenderer(IServiceRegistry services, string backgroundTexturePath)
        //{
        //    // load the background texture
        //    if (!string.IsNullOrEmpty(backgroundTexturePath))
        //    {
        //        var assetManager = (IAssetManager)Services.GetService(typeof(IAssetManager));
        //        BackgroundTexture = assetManager.Load<Texture>(backgroundTexturePath);
        //    }

        //    spriteBatch = new SpriteBatch(GraphicsDevice);
        //}

        protected override void PrepareCore(RenderContext context, RenderItemCollection opaqueList, RenderItemCollection transparentList)
        {
            throw new NotImplementedException();
        }

        protected override void DrawCore(RenderContext context, RenderItemCollection renderItems, int fromIndex, int toIndex)
        {
            throw new NotImplementedException("TODO: REFACTOR THIS CODE TO USE A BackgroundComponent");
            Texture BackgroundTexture = null;
            if (BackgroundTexture == null)
                return;

            var graphicsDevice = context.GraphicsDevice;

            var destination = new RectangleF(0, 0, graphicsDevice.BackBuffer.ViewWidth, graphicsDevice.BackBuffer.ViewHeight);

            var imageBufferMinRatio = Math.Min(BackgroundTexture.ViewWidth / (float)graphicsDevice.BackBuffer.ViewWidth, BackgroundTexture.ViewHeight / (float)graphicsDevice.BackBuffer.ViewHeight);
            var sourceSize = new Int2((int)(graphicsDevice.BackBuffer.ViewWidth * imageBufferMinRatio), (int)(graphicsDevice.BackBuffer.ViewHeight * imageBufferMinRatio));
            var source = new Rectangle((BackgroundTexture.ViewWidth - sourceSize.X) / 2, (BackgroundTexture.ViewHeight - sourceSize.Y) / 2, sourceSize.X, sourceSize.Y);

            spriteBatch.Begin(SpriteSortMode.FrontToBack, graphicsDevice.BlendStates.Opaque, graphicsDevice.SamplerStates.LinearClamp, graphicsDevice.DepthStencilStates.None);
            spriteBatch.Draw(BackgroundTexture, destination, source, Color.White, 0, Vector2.Zero, SpriteEffects.None, ImageOrientation.AsIs, 0);
            spriteBatch.End();

            // reset the states to default
            graphicsDevice.SetBlendState(null);
            graphicsDevice.SetRasterizerState(null);
            graphicsDevice.SetDepthStencilState(null);
        }
    }
}