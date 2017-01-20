// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Rendering;
using SiliconStudio.Xenko.Rendering.Composers;

namespace JumpyJet
{
    public class JumpyJetRenderer : SceneRendererBase
    {
        // Entities' depth
        private const int Pal0Depth = 0;
        private const int Pal1Depth = 1;
        private const int Pal2Depth = 2;
        private const int Pal3Depth = 3;

        private SpriteBatch spriteBatch;

        private readonly List<BackgroundSection> backgroundParallax = new List<BackgroundSection>();


        public SpriteSheet ParallaxBackgrounds;

        /// <summary>
        /// The main render stage for opaque geometry.
        /// </summary>
        public RenderStage MainRenderStage { get; set; }

        /// <summary>
        /// The transparent render stage for transparent geometry.
        /// </summary>
        public RenderStage TransparentRenderStage { get; set; }

        public void StartScrolling()
        {
            EnableAllParallaxesUpdate(true);
        }

        public void StopScrolling()
        {
            EnableAllParallaxesUpdate(false);
        }

        private void EnableAllParallaxesUpdate(bool isEnable)
        {
            foreach (var pallarax in backgroundParallax)
            {
                pallarax.IsUpdating = isEnable;
            }
        }

        protected override void InitializeCore()
        {
            base.InitializeCore();

            var virtualResolution = new Vector3(GraphicsDevice.Presenter.BackBuffer.Width, GraphicsDevice.Presenter.BackBuffer.Height, 20f);

            // Create Parallax Background
            backgroundParallax.Add(new BackgroundSection(ParallaxBackgrounds.Sprites[0], virtualResolution, GameScript.GameSpeed / 4f, Pal0Depth));
            backgroundParallax.Add(new BackgroundSection(ParallaxBackgrounds.Sprites[1], virtualResolution, GameScript.GameSpeed / 3f, Pal1Depth));
            backgroundParallax.Add(new BackgroundSection(ParallaxBackgrounds.Sprites[2], virtualResolution, GameScript.GameSpeed / 1.5f, Pal2Depth));

            // For pal3Sprite: Ground, move it downward so that its bottom edge is at the bottom screen.
            var screenHeight = virtualResolution.Y;
            var pal3Height = ParallaxBackgrounds.Sprites[3].SizeInPixels.Y;
            backgroundParallax.Add(new BackgroundSection(ParallaxBackgrounds.Sprites[3], virtualResolution, GameScript.GameSpeed, Pal3Depth, Vector2.UnitY * (screenHeight - pal3Height) / 2));

            // allocate the sprite batch in charge of drawing the backgrounds.
            spriteBatch = new SpriteBatch(GraphicsDevice) { VirtualResolution = virtualResolution };
        }

        protected override void CollectCore(RenderContext context)
        {
            // Setup pixel formats for RenderStage
            using (context.SaveRenderOutputAndRestore())
            {
                // Fill RenderStage formats and register render stages to main view
                if (MainRenderStage != null)
                {
                    context.RenderView.RenderStages.Add(MainRenderStage);
                    MainRenderStage.Output = context.RenderOutput;
                }
                if (TransparentRenderStage != null)
                {
                    context.RenderView.RenderStages.Add(TransparentRenderStage);
                    TransparentRenderStage.Output = context.RenderOutput;
                }
            }
        }

        protected override void DrawCore(RenderDrawContext context)
        {
            var renderSystem = context.RenderContext.RenderSystem;

            // Clear
            context.CommandList.Clear(context.CommandList.DepthStencilBuffer, DepthStencilClearOptions.DepthBuffer);

            // Draw parallax background
            spriteBatch.Begin(context.GraphicsContext);

            float elapsedTime = (float) context.RenderContext.Time.Elapsed.TotalSeconds;
            foreach (var pallaraxBackground in backgroundParallax)
                pallaraxBackground.DrawSprite(elapsedTime, spriteBatch);

            spriteBatch.End();

            // Draw [main view | main stage]
            if (MainRenderStage != null)
                renderSystem.Draw(context, context.RenderContext.RenderView, MainRenderStage);

            // Draw [main view | transparent stage]
            if (TransparentRenderStage != null)
                renderSystem.Draw(context, context.RenderContext.RenderView, TransparentRenderStage);
        }
    }
}