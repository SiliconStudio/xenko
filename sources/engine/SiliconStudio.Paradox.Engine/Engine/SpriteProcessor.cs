// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;
using SiliconStudio.Paradox.Effects;
using SiliconStudio.Paradox.EntityModel;
using SiliconStudio.Paradox.Games;

namespace SiliconStudio.Paradox.Engine
{
    /// <summary>
    /// The processor in charge of updating and drawing the entities having sprite components.
    /// </summary>
    internal class SpriteProcessor : EntityProcessor<SpriteProcessor.AssociatedData>
    {
        private RenderSystem renderSystem;

        public SpriteProcessor()
            : base(new PropertyKey[] { SpriteComponent.Key, TransformationComponent.Key })
        {
        }

        public override void OnSystemAdd()
        {
            renderSystem = Services.GetSafeServiceAs<RenderSystem>();
        }

        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);
            
            // Update the entities to render to renderers
            foreach (var renderProcessor in renderSystem.SpriteRenderProcessors)
            {
                renderProcessor.EntitiesToRender.Clear();
                foreach (var entity in enabledEntities.Keys)
                    renderProcessor.EntitiesToRender.Add(entity);
            }
        }

        protected override AssociatedData GenerateAssociatedData(Entity entity)
        {
            return new AssociatedData
            {
                SpriteComponent = entity.Get(SpriteComponent.Key),
                TransformationComponent = entity.Get(TransformationComponent.Key),
            };
        }

        public class AssociatedData
        {
            public SpriteComponent SpriteComponent;

            public TransformationComponent TransformationComponent;
        }
    }
}