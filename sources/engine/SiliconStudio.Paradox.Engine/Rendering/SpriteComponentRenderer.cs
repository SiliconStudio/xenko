// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.Engine;
using SiliconStudio.Paradox.Graphics;
using SiliconStudio.Paradox.Rendering.Sprites;

namespace SiliconStudio.Paradox.Rendering
{
    /// <summary>
    /// This <see cref="EntityComponentRendererBase"/> is responsible to prepare and render sprites for a specific pass.
    /// </summary>
    public class SpriteComponentRenderer : EntityComponentRendererBase
    {
        // TODO this is temporary code. this should disappear from here later when materials on sprite will be available
        public static PropertyKey<bool> IsEntitySelected = new PropertyKey<bool>("IsEntitySelected", typeof(SpriteComponentRenderer));
        private Effect selectedSpriteEffect;

        private Sprite3DBatch sprite3DBatch;

        private SpriteProcessor spriteProcessor;

        public override bool SupportPicking { get { return true; } }

        public override bool IsWritingToDepth
        {
            get
            {
                return false;
            }
        }

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
                if(sprite == null)
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
            // TODO: Check how to integrate sprites in a Camera renderer instead of this
            var viewParameters = context.Parameters;

            var device = context.GraphicsDevice;
            var cullMode = device.RasterizerStates.CullNone;
            var viewInverse = Matrix.Invert(viewParameters.Get(TransformationKeys.View));
            var viewProjection = viewParameters.Get(TransformationKeys.ViewProjection);

            BlendState previousBlendState = null;
            Effect previousEffect = null;

            var isPicking = context.IsPicking();

            bool hasBegin = false;
            for (var i = fromIndex; i <= toIndex; i++)
            {
                var renderItem = renderItems[i];
                var spriteState = (SpriteProcessor.SpriteComponentState)renderItem.DrawContext;
                var spriteComp = spriteState.SpriteComponent;
                var transfoComp = spriteState.TransformComponent;

                var sprite = spriteComp.CurrentSprite;
                if (sprite == null)
                    continue;

                // Update the sprite batch
                var blendState = isPicking ? device.BlendStates.Opaque : renderItems.HasTransparency ? (spriteComp.PremultipliedAlpha ? device.BlendStates.AlphaBlend : device.BlendStates.NonPremultiplied) : device.BlendStates.Opaque;
                var currentEffect = (!isPicking && spriteComp.Tags.Get(IsEntitySelected)) ? GetOrCreateSelectedSpriteEffect(): null; // TODO remove this code when material are available
                if (previousEffect != currentEffect || blendState != previousBlendState)
                {
                    if (hasBegin)
                    {
                        sprite3DBatch.End();
                    }
                    sprite3DBatch.Begin(viewProjection, SpriteSortMode.Deferred, blendState, null, context.GraphicsDevice.DepthStencilStates.None, cullMode, currentEffect);
                    hasBegin = true;
                }
                previousEffect = currentEffect;
                previousBlendState = blendState;

                var sourceRegion = sprite.Region; 
                var texture = sprite.Texture;
                var color = spriteComp.Color;
                if (isPicking) // TODO move this code corresponding to picking out of the runtime code.
                {
                    texture = device.GetSharedWhiteTexture();
                    color = (Color)new Color4(spriteComp.Id);
                }

                // skip the sprite if no texture is set.
                if (texture == null)
                    continue;

                // determine the size of the element depending on the extrusion method.
                var elementSize = Vector2.One;
                if (spriteComp.ExtrusionMethod == SpriteExtrusionMethod.UnitHeightSpriteRatio)
                {
                    elementSize.X = sourceRegion.Width / sourceRegion.Height;
                }
                else if (spriteComp.ExtrusionMethod == SpriteExtrusionMethod.UnitWidthSpriteRatio)
                {
                    elementSize.Y = sourceRegion.Height / sourceRegion.Width;
                }

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
                
                // draw the sprite
                sprite3DBatch.Draw(texture, ref worldMatrix, ref sourceRegion, ref elementSize, ref color, sprite.Orientation, SwizzleMode.None, renderItem.Depth);
            }

            sprite3DBatch.End();
        }

        private Effect GetOrCreateSelectedSpriteEffect()
        {
            if(selectedSpriteEffect == null)
                selectedSpriteEffect = EffectSystem.LoadEffect("SelectedSprite").WaitForResult();

            return selectedSpriteEffect;
        }

        protected override void Unload()
        {
            sprite3DBatch.Dispose();

            base.Unload();
        }
    }
}