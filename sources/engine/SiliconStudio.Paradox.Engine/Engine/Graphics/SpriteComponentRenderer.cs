// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;

using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.Engine;
using SiliconStudio.Paradox.Engine.Graphics;
using SiliconStudio.Paradox.Engine.Graphics.Composers;
using SiliconStudio.Paradox.EntityModel;
using SiliconStudio.Paradox.Games;
using SiliconStudio.Paradox.Graphics;

namespace SiliconStudio.Paradox.Effects
{
    /// <summary>
    /// This <see cref="EntityComponentRendererBase"/> is responsible to prepare and render sprites for a specific pass.
    /// </summary>
    public class SpriteComponentRenderer : EntityComponentRendererBase
    {
        private SpriteBatch spriteBatch;

        private IVirtualResolution gameVirtualResolution;

        private SpriteProcessor spriteProcessor;

        private static readonly Dictionary<string, List<Entity>> effectNamesToEntityDatas = new Dictionary<string, List<Entity>>();

        public SpriteComponentRenderer()
        {
        }

        private void GameVirtualResolutionChanged(object sender, EventArgs eventArgs)
        {
            spriteBatch.VirtualResolution = gameVirtualResolution.VirtualResolution;
        }

        protected override void InitializeCore()
        {
            base.InitializeCore();

            gameVirtualResolution = (IVirtualResolution)Services.GetService(typeof(IVirtualResolution));

            spriteBatch = new SpriteBatch(Context.GraphicsDevice);

            gameVirtualResolution.VirtualResolutionChanged += GameVirtualResolutionChanged;
            GameVirtualResolutionChanged(null, EventArgs.Empty);
        }

        protected override void PrepareCore(RenderContext context, RenderItemCollection opaqueList, RenderItemCollection transparentList)
        {
            spriteProcessor = SceneInstance.GetProcessor<SpriteProcessor>();
            if (spriteProcessor == null)
            {
                return;
            }

            foreach (var sprite in spriteProcessor.Sprites)
            {
                // Project the position
                // TODO: This could be done in a SIMD batch, but we need to figure-out how to plugin in with RenderMesh object
                var worldPosition = new Vector4(sprite.TransformComponent.WorldMatrix.TranslationVector, 1.0f);

                Vector4 projectedPosition;
                Vector4.Transform(ref worldPosition, ref context.ViewProjectionMatrix, out projectedPosition);
                var projectedZ = projectedPosition.Z / projectedPosition.W;

                var list = sprite.SpriteComponent.CurrentSprite.IsTransparent ? transparentList : opaqueList;

                list.Add(new RenderItem(this, sprite, projectedZ));
            }
        }

        protected override void DrawCore(RenderContext context, RenderItemCollection renderItems, int fromIndex, int toIndex)
        {
            // TODO: Check how to integrate sprites in a Camera renderer instead of this
            var blendState = renderItems.HasTransparency ? context.GraphicsDevice.BlendStates.AlphaBlend : context.GraphicsDevice.BlendStates.Opaque;
            SelectAndSortEntitiesByEffects(renderItems, fromIndex, toIndex);
            DrawSprites(context, SpriteSortMode.FrontToBack, blendState);
        }

        protected override void Unload()
        {
            gameVirtualResolution.VirtualResolutionChanged -= GameVirtualResolutionChanged;

            spriteBatch.Dispose();

            base.Unload();
        }

        private void SelectAndSortEntitiesByEffects(List<RenderItem> renderItemList, int fromIndex, int toIndex)
        {
            // clear current cache
            foreach (var entities in effectNamesToEntityDatas.Values)
                entities.Clear();

            // select and sort the entities
            for(int i = fromIndex; i <= toIndex; i++)
            {
                var spriteComp = (SpriteComponent)renderItemList[i].DrawContext;
                var entity = spriteComp.Entity;

                if (spriteComp.SpriteGroup == null || spriteComp.SpriteGroup.Images == null)
                    continue;

                var effectName = spriteComp.Effect != null ? spriteComp.Effect.Name : "SpriteBatch.DefaultEffect";

                if (!effectNamesToEntityDatas.ContainsKey(effectName))
                    effectNamesToEntityDatas.Add(effectName, new List<Entity>());

                effectNamesToEntityDatas[effectName].Add(entity);
            }
        }

        private void DrawSprites(RenderContext context, SpriteSortMode sortMode, BlendState blendState)
        {
            var viewParameters = context.Parameters;

            var viewMatrix = viewParameters.Get(TransformationKeys.View);
            var projectionMatrix = viewParameters.Get(TransformationKeys.Projection);

            foreach (var entities in effectNamesToEntityDatas.Values)
            {
                if (entities.Count == 0)
                    continue;

                spriteBatch.Begin(viewMatrix, projectionMatrix, sortMode, blendState, effect: entities[0].Get(SpriteComponent.Key).Effect);

                foreach (var entity in entities)
                {
                    var spriteComp = entity.Get(SpriteComponent.Key);
                    var transfoComp = entity.Get(TransformComponent.Key);

                    var sprite = spriteComp.CurrentSprite;
                    if (sprite == null)
                        continue;

                    sprite.Draw(
                        spriteBatch,
                        new Vector2(transfoComp.Position.X, transfoComp.Position.Y),
                        spriteComp.Color,
                        new Vector2(transfoComp.Scale.X, transfoComp.Scale.Y),
                        transfoComp.RotationEulerXYZ.Z,
                        transfoComp.Position.Z,
                        spriteComp.SpriteEffect);
                }

                spriteBatch.End();
            }
        }
    }
}