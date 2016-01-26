using System.Collections.Generic;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Rendering;

namespace RenderArchitecture
{
    /// <summary>
    /// The processor in charge of updating and drawing the entities having sprite components.
    /// </summary>
    internal class NextGenSpriteProcessor : EntityProcessor<RenderSprite>
    {
        private NextGenRenderSystem renderSystem;

        /// <summary>
        /// Initializes a new instance of the <see cref="NextGenSpriteProcessor"/> class.
        /// </summary>
        public NextGenSpriteProcessor(NextGenRenderSystem renderSystem)
            : base(SpriteComponent.Key, TransformComponent.Key)
        {
            this.renderSystem = renderSystem;
            Sprites = new List<RenderSprite>();
        }

        public List<RenderSprite> Sprites { get; private set; }

        public override void Draw(RenderContext gameTime)
        {
            Sprites.Clear();
            foreach (var spriteStateKeyPair in enabledEntities)
            {
                if (spriteStateKeyPair.Value.SpriteComponent.Enabled)
                {
                    Sprites.Add(spriteStateKeyPair.Value);
                }
            }
        }

        protected override void OnEntityAdding(Entity entity, RenderSprite data)
        {
            renderSystem.RenderObjects.Add(data);
        }

        protected override void OnEntityRemoved(Entity entity, RenderSprite data)
        {
            renderSystem.RenderObjects.Remove(data);
        }

        protected override RenderSprite GenerateAssociatedData(Entity entity)
        {
            return new RenderSprite
            {
                SpriteComponent = entity.Get(SpriteComponent.Key),
                TransformComponent = entity.Get(TransformComponent.Key),
            };
        }

        protected override bool IsAssociatedDataValid(Entity entity, RenderSprite associatedData)
        {
            return
                entity.Get(SpriteComponent.Key) == associatedData.SpriteComponent &&
                entity.Get(TransformComponent.Key) == associatedData.TransformComponent;
        }
    }
}