// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;

using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Core.Serialization.Assets;
using SiliconStudio.Paradox.Graphics;

namespace SiliconStudio.Paradox.Effects
{
    /// <summary>
    /// This renderer draws a full-screen image as background. 
    /// The ratio or the texture used is preserved. The texture is centered and cropped along X or Y axis depending on the screen ratio.
    /// </summary>
    /// <remarks>This renderer does not write into the depth buffer</remarks>
    public class BackgroundRenderer : Renderer
    {
        private readonly SpriteBatch spriteBatch;

        /// <summary>
        /// Gets or sets the texture displayed as background.
        /// </summary>
        public Texture BackgroundTexture { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="BackgroundRenderer"/> with null texture.
        /// </summary>
        /// <param name="services">The services.</param>
        public BackgroundRenderer(IServiceRegistry services)
            : this(services, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BackgroundRenderer"/> using the provided file as background texture.
        /// </summary>
        /// <param name="services">The services.</param>
        /// <param name="backgroundTexturePath">The path to the background texture to use</param>
        public BackgroundRenderer(IServiceRegistry services, string backgroundTexturePath)
            : base(services)
        {
            // load the background texture
            if (!string.IsNullOrEmpty(backgroundTexturePath))
            {
                var assetManager = (IAssetManager)Services.GetService(typeof(IAssetManager));
                BackgroundTexture = assetManager.Load<Texture>(backgroundTexturePath);
            }

            spriteBatch = new SpriteBatch(GraphicsDevice);
        }

        protected override void OnRendering(RenderContext context)
        {
            if(BackgroundTexture == null)
                return;

            var destination = new RectangleF(0, 0, GraphicsDevice.BackBuffer.ViewWidth, GraphicsDevice.BackBuffer.ViewHeight);

            var imageBufferMinRatio = Math.Min(BackgroundTexture.ViewWidth / (float)GraphicsDevice.BackBuffer.ViewWidth, BackgroundTexture.ViewHeight / (float)GraphicsDevice.BackBuffer.ViewHeight);
            var sourceSize = new Int2((int)(GraphicsDevice.BackBuffer.ViewWidth * imageBufferMinRatio), (int)(GraphicsDevice.BackBuffer.ViewHeight * imageBufferMinRatio));
            var source = new Rectangle((BackgroundTexture.ViewWidth - sourceSize.X) / 2, (BackgroundTexture.ViewHeight - sourceSize.Y) / 2, sourceSize.X, sourceSize.Y);

            spriteBatch.Begin(SpriteSortMode.FrontToBack, GraphicsDevice.BlendStates.Opaque, GraphicsDevice.SamplerStates.LinearClamp, GraphicsDevice.DepthStencilStates.None);
            spriteBatch.Draw(BackgroundTexture, destination, source, Color.White, 0, Vector2.Zero, SpriteEffects.None, ImageOrientation.AsIs, 0);
            spriteBatch.End();

            // reset the states to default
            GraphicsDevice.SetBlendState(null);
            GraphicsDevice.SetRasterizerState(null);
            GraphicsDevice.SetDepthStencilState(null);
        }
    }
}