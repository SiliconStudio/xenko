// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;

using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.Engine;
using SiliconStudio.Paradox.EntityModel;
using SiliconStudio.Paradox.Games;
using SiliconStudio.Paradox.Graphics;

namespace SiliconStudio.Paradox.Effects
{
    /// <summary>
    /// This <see cref="Renderer"/> is responsible to prepare and render sprites for a specific pass.
    /// </summary>
    public class SpriteRenderer : Renderer
    {
        private SpriteBatch spriteBatch;

        private readonly IVirtualResolution gameVirtualResolution;

        private readonly RenderSystem renderSystem;

        internal readonly List<Entity> EntitiesToRender = new List<Entity>();

        private static readonly Dictionary<string, List<Entity>> effectNamesToEntityDatas = new Dictionary<string, List<Entity>>();

        public SpriteRenderer(IServiceRegistry services)
            : base(services)
        {
            renderSystem = (RenderSystem)services.GetService(typeof(RenderSystem));
            gameVirtualResolution = (IVirtualResolution)services.GetService(typeof(IVirtualResolution));
        }

        private void GameVirtualResolutionChanged(object sender, EventArgs eventArgs)
        {
            spriteBatch.VirtualResolution = gameVirtualResolution.VirtualResolution;
        }

        public override void Load()
        {
            base.Load();

            spriteBatch = new SpriteBatch(GraphicsDevice);

            gameVirtualResolution.VirtualResolutionChanged += GameVirtualResolutionChanged;
            GameVirtualResolutionChanged(null, EventArgs.Empty);

            renderSystem.SpriteRenderProcessors.Add(this);
        }

        public override void Unload()
        {
            base.Unload();

            renderSystem.SpriteRenderProcessors.Remove(this);

            gameVirtualResolution.VirtualResolutionChanged -= GameVirtualResolutionChanged;

            spriteBatch.Dispose();
        }

        protected override void OnRendering(RenderContext context)
        {
            // draw opaque sprites 
            SelectAndSortEntitiesByEffects(SpriteIsOpaque);
            DrawSprites(context, SpriteSortMode.FrontToBack, GraphicsDevice.BlendStates.Opaque);

            // draw transparent objects
            SelectAndSortEntitiesByEffects(SpriteIsTransparent);
            DrawSprites(context, SpriteSortMode.BackToFront, GraphicsDevice.BlendStates.AlphaBlend);
        }

        private bool SpriteIsTransparent(SpriteComponent spriteComponent)
        {
            return spriteComponent.CurrentSprite.IsTransparent;
        }

        private bool SpriteIsOpaque(SpriteComponent spriteComponent)
        {
            return !SpriteIsTransparent(spriteComponent);
        }

        private void SelectAndSortEntitiesByEffects(Func<SpriteComponent, bool> shouldSelect)
        {
            // clear current cache
            foreach (var entities in effectNamesToEntityDatas.Values)
                entities.Clear();

            // select and sort the entities
            foreach (var entity in EntitiesToRender)
            {
                var spriteComp = entity.Get(SpriteComponent.Key);

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
            var viewParameters = context.CurrentPass.Parameters;

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
                    var transfoComp = entity.Get(TransformationComponent.Key);

                    var sprite = spriteComp.CurrentSprite;
                    if (sprite == null)
                        continue;

                    sprite.Draw(
                        spriteBatch,
                        new Vector2(transfoComp.Translation.X, transfoComp.Translation.Y),
                        spriteComp.Color,
                        new Vector2(transfoComp.Scaling.X, transfoComp.Scaling.Y),
                        transfoComp.RotationEulerXYZ.Z,
                        transfoComp.Translation.Z,
                        spriteComp.SpriteEffect);
                }

                spriteBatch.End();
            }
        }
    }
}