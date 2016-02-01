// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Rendering.Sprites;

namespace SiliconStudio.Xenko.Rendering.Sprites
{
    public class SpriteRenderFeature : RootRenderFeature
    {
        private Sprite3DBatch sprite3DBatch;

        public override bool SupportsRenderObject(RenderObject renderObject)
        {
            return renderObject is RenderSprite;
        }

        public override void Initialize()
        {
            base.Initialize();

            sprite3DBatch = new Sprite3DBatch(RenderSystem.GraphicsDevice);
        }

        public override void Draw(RenderView renderView, RenderViewStage renderViewStage, int startIndex, int endIndex)
        {
            base.Draw(renderView, renderViewStage, startIndex, endIndex);

            Matrix viewInverse;
            Matrix.Invert(ref renderView.View, out viewInverse);

            BlendState previousBlendState = null;
            DepthStencilState previousDepthStencilState = null;
            EffectInstance previousEffect = null;

            var isPicking = false; //context.IsPicking();

            var device = RenderSystem.GraphicsDevice;

            bool hasBegin = false;
            for (var index = startIndex; index < endIndex; index++)
            {
                var renderNodeReference = renderViewStage.RenderNodes[index].RenderNode;
                var renderNode = GetRenderNode(renderNodeReference);

                var renderSprite = (RenderSprite)renderNode.RenderObject;

                var spriteComp = renderSprite.SpriteComponent;
                var transfoComp = renderSprite.TransformComponent;
                var depthStencilState = renderSprite.SpriteComponent.IgnoreDepth ? device.DepthStencilStates.None : device.DepthStencilStates.Default;

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


                // Update the sprite batch
                var blendState = isPicking ? device.BlendStates.Opaque : sprite.IsTransparent ? (spriteComp.PremultipliedAlpha ? device.BlendStates.AlphaBlend : device.BlendStates.NonPremultiplied) : device.BlendStates.Opaque;
                var currentEffect = isPicking ? GetOrCreatePickingSpriteEffect() : /*spriteComp.Tags.Get(IsEntitySelected) ? GetOrCreateSelectedSpriteEffect() :*/ null; // TODO remove this code when material are available
                if (previousEffect != currentEffect || blendState != previousBlendState || depthStencilState != previousDepthStencilState)
                {
                    if (hasBegin)
                    {
                        sprite3DBatch.End();
                    }
                    sprite3DBatch.Begin(renderView.ViewProjection, SpriteSortMode.Deferred, blendState, null, depthStencilState, device.RasterizerStates.CullNone, currentEffect);
                    hasBegin = true;
                }
                previousEffect = currentEffect;
                previousBlendState = blendState;
                previousDepthStencilState = depthStencilState;

                var sourceRegion = sprite.Region;
                var texture = sprite.Texture;
                var color = spriteComp.Color;
                if (isPicking) // TODO move this code corresponding to picking out of the runtime code.
                    color = new Color4(RuntimeIdHelper.ToRuntimeId(spriteComp));

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

            sprite3DBatch.End();
        }

        private EffectInstance GetOrCreatePickingSpriteEffect()
        {
            throw new System.NotImplementedException();
        }
    }
}