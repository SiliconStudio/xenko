using System;
using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Core.Reflection;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Rendering;

namespace SiliconStudio.Xenko.SpriteStudio.Runtime
{
    //TODO this whole renderer is not optimized at all! batching is wrong and depth calculation should be done differently
    public class SpriteStudioRenderer : EntityComponentRendererBase
    {
        // TODO this is temporary code. this should disappear from here later when materials on sprite will be available
        public static PropertyKey<bool> IsEntitySelected = new PropertyKey<bool>("IsEntitySelected", typeof(SpriteStudioRenderer));

        private EffectInstance selectedSpriteEffect;
        private EffectInstance pickingSpriteEffect;

        private Sprite3DBatch sprite3DBatch;

        private SpriteStudioProcessor spriteProcessor;

        public override bool SupportPicking => true;

        public BlendStateDescription MultBlendState;
        public BlendStateDescription SubBlendState;

        protected override void InitializeCore()
        {
            base.InitializeCore();

            sprite3DBatch = new Sprite3DBatch(Context.GraphicsDevice);

            var blendDesc = new BlendStateDescription(Blend.SourceAlpha, Blend.One);
            blendDesc.RenderTargets[0].BlendEnable = true;
            blendDesc.RenderTargets[0].ColorBlendFunction = BlendFunction.ReverseSubtract;
            blendDesc.RenderTargets[0].AlphaBlendFunction = BlendFunction.ReverseSubtract;
            SubBlendState = blendDesc;

            blendDesc = new BlendStateDescription(Blend.DestinationColor, Blend.InverseSourceAlpha);
            blendDesc.RenderTargets[0].BlendEnable = true;
            blendDesc.RenderTargets[0].ColorBlendFunction = BlendFunction.Add;
            blendDesc.RenderTargets[0].AlphaSourceBlend = Blend.Zero;
            blendDesc.RenderTargets[0].AlphaBlendFunction = BlendFunction.Add;
            MultBlendState = blendDesc;
        }

        protected override void PrepareCore(RenderDrawContext context, RenderItemCollection opaqueList, RenderItemCollection transparentList)
        {
            spriteProcessor = SceneInstance.GetProcessor<SpriteStudioProcessor>();
            if (spriteProcessor == null)
            {
                return;
            }

            // If no camera, early exit
            var camera = context.RenderContext.GetCurrentCamera();
            if (camera == null)
            {
                return;
            }
            var viewProjectionMatrix = camera.ViewProjectionMatrix;

            foreach (var spriteState in spriteProcessor.Sprites)
            {
                var worldMatrix = spriteState.TransformComponent.WorldMatrix;

                var worldPosition = new Vector4(worldMatrix.TranslationVector, 1.0f);

                Vector4 projectedPosition;
                Vector4.Transform(ref worldPosition, ref viewProjectionMatrix, out projectedPosition);
                var projectedZ = projectedPosition.Z / projectedPosition.W;

                transparentList.Add(new RenderItem(this, spriteState, projectedZ));

                //for (var index = 0; index < ssSheet.Sheet.NodesInfo.Count; index++)
                //{
                //    var node = ssSheet.Sheet.NodesInfo[index];
                //    var sprite = ssSheet.Sheet.SpriteSheet.Sprites[index];

                //    if (sprite?.Texture == null || sprite.Region.Width <= 0 || sprite.Region.Height <= 0f)
                //        continue;

                //    // Perform culling on group and accept
                //    if (!CurrentCullingMask.Contains(spriteState.SpriteStudioComponent.Entity.Group))
                //        continue;

                //    var worldMatrix = node.WorldTransform * spriteState.TransformComponent.WorldMatrix;

                //    // Project the position
                //    // TODO: This could be done in a SIMD batch, but we need to figure-out how to plugin in with RenderMesh object
                //    var worldPosition = new Vector4(worldMatrix.TranslationVector, 1.0f);

                //    Vector4 projectedPosition;
                //    Vector4.Transform(ref worldPosition, ref viewProjectionMatrix, out projectedPosition);
                //    var projectedZ = projectedPosition.Z / projectedPosition.W;

                //    var list = sprite.IsTransparent ? transparentList : opaqueList;

                //    list.Add(new RenderItem(this, new SpriteItem
                //    {
                //        Sprite = sprite,
                //        Data = spriteState,
                //        Node = node
                //    }, projectedZ));
                //}
            }
        }

        protected override void DrawCore(RenderDrawContext context, RenderItemCollection renderItems, int fromIndex, int toIndex)
        {
            //var viewParameters = context.Parameters;

            var device = context.GraphicsDevice;
            // TODO GRAPHICS REFACTOR probably better to receive RenderView when reimplemented
            var cameraState = context.RenderContext.GetCurrentCamera();
            if (cameraState == null) throw new InvalidOperationException("No valid camera");
            var viewProjection = cameraState.ViewProjectionMatrix; // viewParameters.Get(TransformationKeys.ViewProjection);

            BlendStateDescription? previousBlendState = null;
            DepthStencilStateDescription? previousDepthStencilState = null;
            EffectInstance previousEffect = null;

            var isPicking = context.RenderContext.IsPicking();

            bool hasBegin = false;
            for (var i = fromIndex; i <= toIndex; i++)
            {
                var renderItem = renderItems[i];
                var spriteState = (SpriteStudioProcessor.Data)renderItem.DrawContext;
                var transfoComp = spriteState.TransformComponent;
                var depthStencilState = device.DepthStencilStates.None;

                foreach (var node in spriteState.SpriteStudioComponent.SortedNodes)
                {
                    if (node.Sprite?.Texture == null || node.Sprite.Region.Width <= 0 || node.Sprite.Region.Height <= 0f || node.Hide != 0) continue;

                    // Update the sprite batch

                    BlendStateDescription spriteBlending;
                    switch (node.BaseNode.AlphaBlending)
                    {
                        case SpriteStudioBlending.Mix:
                            spriteBlending = device.BlendStates.AlphaBlend;
                            break;
                        case SpriteStudioBlending.Multiplication:
                            spriteBlending = MultBlendState;
                            break;
                        case SpriteStudioBlending.Addition:
                            spriteBlending = device.BlendStates.Additive;
                            break;
                        case SpriteStudioBlending.Subtraction:
                            spriteBlending = SubBlendState;
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                    var blendState = isPicking ? device.BlendStates.Opaque : renderItems.HasTransparency ? spriteBlending : device.BlendStates.Opaque;
                    var currentEffect = isPicking ? GetOrCreatePickingSpriteEffect() : ShadowObject.IsObjectSelected(spriteState.SpriteStudioComponent) ? GetOrCreateSelectedSpriteEffect() : null;
                    // TODO remove this code when material are available
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

                    var sourceRegion = node.Sprite.Region;
                    var texture = node.Sprite.Texture;

                    // skip the sprite if no texture is set.
                    if (texture == null)
                        continue;

                    var color4 = Color4.White;
                    if (isPicking)
                    {
                        // TODO move this code corresponding to picking out of the runtime code.
                        color4 = new Color4(RuntimeIdHelper.ToRuntimeId(spriteState.SpriteStudioComponent));
                    }
                    else
                    {
                        if (node.BlendFactor > 0.0f)
                        {
                            switch (node.BlendType) //todo this should be done in a shader
                            {
                                case SpriteStudioBlending.Mix:
                                    color4 = Color4.Lerp(color4, node.BlendColor, node.BlendFactor) * node.FinalTransparency;
                                    break;
                                case SpriteStudioBlending.Multiplication:
                                    color4 = Color4.Lerp(color4, node.BlendColor, node.BlendFactor) * node.FinalTransparency;
                                    break;
                                case SpriteStudioBlending.Addition:
                                    color4 = Color4.Lerp(color4, node.BlendColor, node.BlendFactor) * node.FinalTransparency;
                                    break;
                                case SpriteStudioBlending.Subtraction:
                                    color4 = Color4.Lerp(color4, node.BlendColor, node.BlendFactor) * node.FinalTransparency;
                                    break;
                                default:
                                    throw new ArgumentOutOfRangeException();
                            }
                        }
                        else
                        {
                            color4 *= node.FinalTransparency;
                        }
                    }

                    var worldMatrix = node.ModelTransform*transfoComp.WorldMatrix;

                    // calculate normalized position of the center of the sprite (takes into account the possible rotation of the image)
                    var normalizedCenter = new Vector2(node.Sprite.Center.X/sourceRegion.Width - 0.5f, 0.5f - node.Sprite.Center.Y/sourceRegion.Height);
                    if (node.Sprite.Orientation == ImageOrientation.Rotated90)
                    {
                        var oldCenterX = normalizedCenter.X;
                        normalizedCenter.X = -normalizedCenter.Y;
                        normalizedCenter.Y = oldCenterX;
                    }
                    // apply the offset due to the center of the sprite
                    var size = node.Sprite.Size;
                    var centerOffset = Vector2.Modulate(normalizedCenter, size);
                    worldMatrix.M41 -= centerOffset.X*worldMatrix.M11 + centerOffset.Y*worldMatrix.M21;
                    worldMatrix.M42 -= centerOffset.X*worldMatrix.M12 + centerOffset.Y*worldMatrix.M22;

                    // draw the sprite
                    sprite3DBatch.Draw(texture, ref worldMatrix, ref sourceRegion, ref size, ref color4, node.Sprite.Orientation, SwizzleMode.None, renderItem.Depth);
                }
            }

            sprite3DBatch.End();
        }

        private EffectInstance GetOrCreateSelectedSpriteEffect()
        {
            if (selectedSpriteEffect == null)
                selectedSpriteEffect = new EffectInstance(EffectSystem.LoadEffect("SelectedSprite").WaitForResult());

            return selectedSpriteEffect;
        }

        private EffectInstance GetOrCreatePickingSpriteEffect()
        {
            if (pickingSpriteEffect == null)
                pickingSpriteEffect = new EffectInstance(EffectSystem.LoadEffect("SpritePicking").WaitForResult());

            return pickingSpriteEffect;
        }

        protected override void Unload()
        {
            sprite3DBatch.Dispose();

            base.Unload();
        }
    }
}