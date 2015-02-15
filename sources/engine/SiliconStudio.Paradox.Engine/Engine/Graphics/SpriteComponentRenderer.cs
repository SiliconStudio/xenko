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

        public override void Load(RenderContext context)
        {
            base.Load(context);

            gameVirtualResolution = (IVirtualResolution)Services.GetService(typeof(IVirtualResolution));

            spriteBatch = new SpriteBatch(context.GraphicsDevice);

            spriteProcessor = SceneInstance.GetProcessor<SpriteProcessor>();

            gameVirtualResolution.VirtualResolutionChanged += GameVirtualResolutionChanged;
            GameVirtualResolutionChanged(null, EventArgs.Empty);
        }

        public override void Unload()
        {
            gameVirtualResolution.VirtualResolutionChanged -= GameVirtualResolutionChanged;

            spriteBatch.Dispose();

            base.Unload();
        }

        protected override void DrawCore(RenderContext context)
        {
            // TODO: Check how to integrate sprites in a Camera renderer instead of this
            // draw opaque sprites 
            SelectAndSortEntitiesByEffects(spriteProcessor, SpriteIsOpaque);
            DrawSprites(context, SpriteSortMode.FrontToBack, context.GraphicsDevice.BlendStates.Opaque);

            // draw transparent objects
            SelectAndSortEntitiesByEffects(spriteProcessor, SpriteIsTransparent);
            DrawSprites(context, SpriteSortMode.BackToFront, context.GraphicsDevice.BlendStates.AlphaBlend);
        }

        private bool SpriteIsTransparent(SpriteComponent spriteComponent)
        {
            return spriteComponent.CurrentSprite.IsTransparent;
        }

        private bool SpriteIsOpaque(SpriteComponent spriteComponent)
        {
            return !SpriteIsTransparent(spriteComponent);
        }

        private void SelectAndSortEntitiesByEffects(SpriteProcessor spriteProcessor, Func<SpriteComponent, bool> shouldSelect)
        {
            // clear current cache
            foreach (var entities in effectNamesToEntityDatas.Values)
                entities.Clear();

            // select and sort the entities
            foreach (var entityKeyPair in spriteProcessor.Sprites)
            {
                var spriteComp = entityKeyPair.SpriteComponent;
                var entity = spriteComp.Entity;

                if (spriteComp.SpriteGroup == null || spriteComp.SpriteGroup.Images == null || !shouldSelect(spriteComp)) 
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