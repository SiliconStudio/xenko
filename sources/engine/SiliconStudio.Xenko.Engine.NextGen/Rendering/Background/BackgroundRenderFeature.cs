// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Linq;
using SiliconStudio.Core;
using SiliconStudio.Core.Collections;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Shaders;

namespace SiliconStudio.Xenko.Rendering.Background
{
    public class BackgroundRenderFeature : RootRenderFeature
    {
        private SpriteBatch spriteBatch;
        private EffectInstance backgroundEffect;

        public override bool SupportsRenderObject(RenderObject renderObject)
        {
            return renderObject is RenderBackground;
        }

        public override void Initialize()
        {
            base.Initialize();

            backgroundEffect = new EffectInstance(new Effect(RenderSystem.GraphicsDevice, BackgroundEffect.Bytecode) { Name = "BackgroundEffect" });
            spriteBatch = new SpriteBatch(RenderSystem.GraphicsDevice) { VirtualResolution = new Vector3(1) };
        }

        public override void Draw(RenderContext context, RenderView renderView, RenderViewStage renderViewStage, int startIndex, int endIndex)
        {
            var target = RenderSystem.RenderContextOld.Tags.GetSafe(RenderFrame.Current);
            var graphicsDevice = context.GraphicsDevice;
            var destination = new RectangleF(0, 0, 1, 1);

            graphicsDevice.PushState();

            for (int index = startIndex; index < endIndex; index++)
            {
                var renderNodeReference = renderViewStage.RenderNodes[index].RenderNode;
                var renderNode = GetRenderNode(renderNodeReference);
                var renderBackground = (RenderBackground)renderNode.RenderObject;
                var texture = renderBackground.Texture;
                
                var imageBufferMinRatio = Math.Min(texture.ViewWidth / (float)target.Width, texture.ViewHeight / (float)target.Height);
                var sourceSize = new Vector2(target.Width * imageBufferMinRatio, target.Height * imageBufferMinRatio);
                var source = new RectangleF((texture.ViewWidth - sourceSize.X) / 2, (texture.ViewHeight - sourceSize.Y) / 2, sourceSize.X, sourceSize.Y);

                spriteBatch.Begin(SpriteSortMode.FrontToBack, graphicsDevice.BlendStates.Opaque, graphicsDevice.SamplerStates.LinearClamp, graphicsDevice.DepthStencilStates.None, null, backgroundEffect);
                spriteBatch.Parameters.SetValueSlow(BackgroundEffectKeys.Intensity, renderBackground.Intensity);
                spriteBatch.Draw(texture, destination, source, Color.White, 0, Vector2.Zero);
                spriteBatch.End();
            }

            graphicsDevice.PopState();
        }
    }
}