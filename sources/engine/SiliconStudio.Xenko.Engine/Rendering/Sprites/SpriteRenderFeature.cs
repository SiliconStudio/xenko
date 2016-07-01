// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Rendering.Sprites;
using SiliconStudio.Xenko.Shaders.Compiler;

namespace SiliconStudio.Xenko.Rendering.Sprites
{
    public class SpriteRenderFeature : RootRenderFeature
    {
        private Sprite3DBatch sprite3DBatch;

        private EffectInstance pickingSpriteEffect;

        public override Type SupportedRenderObjectType => typeof(RenderSprite);

        protected override void InitializeCore()
        {
            base.InitializeCore();

            sprite3DBatch = new Sprite3DBatch(RenderSystem.GraphicsDevice);
        }

        protected override void Destroy()
        {
            base.Destroy();

            sprite3DBatch.Dispose();
        }

        public override void Draw(RenderDrawContext context, RenderView renderView, RenderViewStage renderViewStage, int startIndex, int endIndex)
        {
            base.Draw(context, renderView, renderViewStage, startIndex, endIndex);

            Matrix viewInverse;
            Matrix.Invert(ref renderView.View, out viewInverse);

            uint previousBatchState = uint.MaxValue;

            //TODO string comparison ...?
            var isPicking = renderViewStage.RenderStage.Name == "Picking";

            bool hasBegin = false;
            for (var index = startIndex; index < endIndex; index++)
            {
                var renderNodeReference = renderViewStage.SortedRenderNodes[index].RenderNode;
                var renderNode = GetRenderNode(renderNodeReference);

                var renderSprite = (RenderSprite)renderNode.RenderObject;

                var spriteComp = renderSprite.SpriteComponent;
                var transfoComp = renderSprite.TransformComponent;

                var sprite = spriteComp.CurrentSprite;
                if (sprite == null)
                    continue;

                // TODO: this should probably be moved to Prepare()
                // Project the position
                // TODO: This could be done in a SIMD batch, but we need to figure-out how to plugin in with RenderMesh object
                var worldPosition = new Vector4(renderSprite.TransformComponent.WorldMatrix.TranslationVector, 1.0f);

                Vector4 projectedPosition;
                Vector4.Transform(ref worldPosition, ref renderView.ViewProjection, out projectedPosition);
                var projectedZ = projectedPosition.Z / projectedPosition.W;

                // Check if the current blend state has changed in any way, if not
                // Note! It doesn't really matter in what order we build the bitmask, the result is not preserved anywhere except in this method
                var currentBatchState = isPicking ? 0U : sprite.IsTransparent ? (spriteComp.PremultipliedAlpha ? 1U : 2U) : 3U;
                currentBatchState = (currentBatchState << 1) + (renderSprite.SpriteComponent.IgnoreDepth ? 1U : 0U);

                if (previousBatchState != currentBatchState)
                {
                    var blendState = isPicking ? BlendStates.Default : sprite.IsTransparent ? (spriteComp.PremultipliedAlpha ? BlendStates.AlphaBlend : BlendStates.NonPremultiplied) : BlendStates.Opaque;
                    var currentEffect = isPicking ? GetOrCreatePickingSpriteEffect() : null; // TODO remove this code when material are available
                    var depthStencilState = renderSprite.SpriteComponent.IgnoreDepth ? DepthStencilStates.None : DepthStencilStates.Default;

                    if (hasBegin)
                    {
                        sprite3DBatch.End();
                    }
                    sprite3DBatch.Begin(context.GraphicsContext, renderView.ViewProjection, SpriteSortMode.Deferred, blendState, null, depthStencilState, RasterizerStates.CullNone, currentEffect);
                    hasBegin = true;
                }
                previousBatchState = currentBatchState;

                var sourceRegion = sprite.Region;
                var texture = sprite.Texture;
                var color = spriteComp.Color;                
                if (isPicking) // TODO move this code corresponding to picking out of the runtime code.
                {
                    var compId = RuntimeIdHelper.ToRuntimeId(spriteComp);
                    color = new Color4(compId);
                }

                // skip the sprite if no texture is set.
                if (texture == null)
                    continue;

                // determine the element world matrix depending on the type of sprite
                var worldMatrix = transfoComp.WorldMatrix;
                if (spriteComp.SpriteType == SpriteType.Billboard)
                {
                    worldMatrix = viewInverse;

                    // remove scale of the camera
                    worldMatrix.Row1 /= ((Vector3)viewInverse.Row1).Length();
                    worldMatrix.Row2 /= ((Vector3)viewInverse.Row2).Length();

                    // set the scale of the object
                    worldMatrix.Row1 *= ((Vector3)transfoComp.WorldMatrix.Row1).Length();
                    worldMatrix.Row2 *= ((Vector3)transfoComp.WorldMatrix.Row2).Length();

                    // set the position
                    worldMatrix.TranslationVector = transfoComp.WorldMatrix.TranslationVector;
                }

                // calculate normalized position of the center of the sprite (takes into account the possible rotation of the image)
                var normalizedCenter = new Vector2(sprite.Center.X / sourceRegion.Width - 0.5f, 0.5f - sprite.Center.Y / sourceRegion.Height);
                if (sprite.Orientation == ImageOrientation.Rotated90)
                {
                    var oldCenterX = normalizedCenter.X;
                    normalizedCenter.X = -normalizedCenter.Y;
                    normalizedCenter.Y = oldCenterX;
                }
                // apply the offset due to the center of the sprite
                var centerOffset = Vector2.Modulate(normalizedCenter, sprite.SizeInternal);
                worldMatrix.M41 -= centerOffset.X * worldMatrix.M11 + centerOffset.Y * worldMatrix.M21;
                worldMatrix.M42 -= centerOffset.X * worldMatrix.M12 + centerOffset.Y * worldMatrix.M22;
                worldMatrix.M43 -= centerOffset.X * worldMatrix.M13 + centerOffset.Y * worldMatrix.M23;

                // draw the sprite
                sprite3DBatch.Draw(texture, ref worldMatrix, ref sourceRegion, ref sprite.SizeInternal, ref color, sprite.Orientation, SwizzleMode.None, projectedZ);
            }

            if(hasBegin) sprite3DBatch.End();
        }

        private EffectInstance GetOrCreatePickingSpriteEffect()
        {
            return pickingSpriteEffect ?? (pickingSpriteEffect = new EffectInstance(RenderSystem.EffectSystem.LoadEffect("SpritePicking").WaitForResult()));
        }
    }
}
