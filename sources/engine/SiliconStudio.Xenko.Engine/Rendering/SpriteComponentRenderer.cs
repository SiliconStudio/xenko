// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Rendering.Sprites;
using SiliconStudio.Xenko.Shaders.Compiler;

namespace SiliconStudio.Xenko.Rendering
{
    /// <summary>
    /// This <see cref="EntityComponentRendererBase"/> is responsible to prepare and render sprites for a specific pass.
    /// </summary>
    public class SpriteComponentRenderer : EntityComponentRendererBase
    {
        // TODO this is temporary code. this should disappear from here later when materials on sprite will be available
        public static PropertyKey<bool> IsEntitySelected = new PropertyKey<bool>("IsEntitySelected", typeof(SpriteComponentRenderer));
        private Effect selectedSpriteEffect;
        private Effect selectedSpriteEffectSRgb;
        private Effect pickingSpriteEffect;

        private Sprite3DBatch sprite3DBatch;

        private SpriteProcessor spriteProcessor;

        public override bool SupportPicking => true;

        protected override void InitializeCore()
        {
            base.InitializeCore();

            sprite3DBatch = new Sprite3DBatch(Context.GraphicsDevice);
        }

        protected override void PrepareCore(RenderContext context, RenderItemCollection opaqueList, RenderItemCollection transparentList)
        {
            spriteProcessor = SceneInstance.GetProcessor<SpriteProcessor>();
            if (spriteProcessor == null)
            {
                return;
            }

            // If no camera, early exit
            var camera = context.GetCurrentCamera();
            if (camera == null)
            {
                return;
            }
            var viewProjectionMatrix = camera.ViewProjectionMatrix;

            foreach (var spriteState in spriteProcessor.Sprites)
            {
                var sprite = spriteState.SpriteComponent.CurrentSprite;
                if(sprite == null || sprite.Texture == null || sprite.Region.Width <= 0 || sprite.Region.Height <= 0f)
                    continue;

                // Perform culling on group and accept
                if (!CurrentCullingMask.Contains(spriteState.SpriteComponent.Entity.Group))
                    continue;

                // Project the position
                // TODO: This could be done in a SIMD batch, but we need to figure-out how to plugin in with RenderMesh object
                var worldPosition = new Vector4(spriteState.TransformComponent.WorldMatrix.TranslationVector, 1.0f);

                Vector4 projectedPosition;
                Vector4.Transform(ref worldPosition, ref viewProjectionMatrix, out projectedPosition);
                var projectedZ = projectedPosition.Z / projectedPosition.W;

                var list = sprite.IsTransparent ? transparentList : opaqueList;

                list.Add(new RenderItem(this, spriteState, projectedZ));
            }
        }

        protected override void DrawCore(RenderContext context, RenderItemCollection renderItems, int fromIndex, int toIndex)
        {
            var viewParameters = context.Parameters;

            var device = context.GraphicsDevice;
            var viewInverse = Matrix.Invert(viewParameters.Get(TransformationKeys.View));
            var viewProjection = viewParameters.Get(TransformationKeys.ViewProjection);

            BlendState previousBlendState = null;
            DepthStencilState previousDepthStencilState= null;
            Effect previousEffect = null;

            var isPicking = context.IsPicking();

            bool hasBegin = false;
            for (var i = fromIndex; i <= toIndex; i++)
            {
                var renderItem = renderItems[i];
                var spriteState = (SpriteProcessor.SpriteComponentState)renderItem.DrawContext;
                var spriteComp = spriteState.SpriteComponent;
                var transfoComp = spriteState.TransformComponent;
                var depthStencilState = spriteState.SpriteComponent.IgnoreDepth ? device.DepthStencilStates.None : device.DepthStencilStates.Default;

                var sprite = spriteComp.CurrentSprite;
                if (sprite == null)
                    continue;

                // Update the sprite batch
                var blendState = isPicking ? device.BlendStates.Opaque : renderItems.HasTransparency ? (spriteComp.PremultipliedAlpha ? device.BlendStates.AlphaBlend : device.BlendStates.NonPremultiplied) : device.BlendStates.Opaque;
                var currentEffect = isPicking? GetOrCreatePickingSpriteEffect(): spriteComp.Tags.Get(IsEntitySelected)? GetOrCreateSelectedSpriteEffect(): null; // TODO remove this code when material are available
                if (previousEffect != currentEffect || blendState != previousBlendState || depthStencilState != previousDepthStencilState)
                {
                    if (hasBegin)
                    {
                        sprite3DBatch.End();
                    }
                    sprite3DBatch.Begin(viewProjection, SpriteSortMode.Deferred, blendState, null, depthStencilState, device.RasterizerStates.CullNone, currentEffect);
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
                sprite3DBatch.Draw(texture, ref worldMatrix, ref sourceRegion, ref sprite.SizeInternal, ref color, sprite.Orientation, SwizzleMode.None, renderItem.Depth);
            }

            sprite3DBatch.End();
        }

        private Effect GetOrCreateSelectedSpriteEffect()
        {
            if (GraphicsDevice.ColorSpace == ColorSpace.Gamma)
            {
                return GetOrCreateSelectedSpriteEffect(ref selectedSpriteEffect, true);
            }
            else
            {
                return GetOrCreateSelectedSpriteEffect(ref selectedSpriteEffectSRgb, false);
            }
        }

        private Effect GetOrCreateSelectedSpriteEffect(ref Effect effect, bool isSRgb)
        {
            if (effect == null)
            {
                var compilerParameters = new CompilerParameters { [SpriteBaseKeys.ColorIsSRgb] = isSRgb};
                effect = EffectSystem.LoadEffect("SelectedSprite", compilerParameters).WaitForResult();
            }

            return effect;
        }

        private Effect GetOrCreatePickingSpriteEffect()
        {
            if (pickingSpriteEffect == null)
                pickingSpriteEffect = EffectSystem.LoadEffect("SpritePicking").WaitForResult();

            return pickingSpriteEffect;
        }

        protected override void Unload()
        {
            sprite3DBatch.Dispose();

            base.Unload();
        }
    }
}