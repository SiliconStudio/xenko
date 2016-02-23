using System.Collections.Generic;
using SiliconStudio.Core;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Rendering;

namespace SiliconStudio.Xenko.Rendering.Sprites
{
    /// <summary>
    /// The processor in charge of updating and drawing the entities having sprite components.
    /// </summary>
    internal class NextGenSpriteProcessor : EntityProcessor<SpriteComponent, RenderSprite>
    {
        private NextGenRenderSystem renderSystem;

        /// <summary>
        /// Initializes a new instance of the <see cref="NextGenSpriteProcessor"/> class.
        /// </summary>
        public NextGenSpriteProcessor()
            : base(typeof(TransformComponent))
        {
        }

        protected internal override void OnSystemAdd()
        {
            renderSystem = Services.GetSafeServiceAs<NextGenRenderSystem>();
        }

        public override void Draw(RenderContext gameTime)
        {
            foreach (var spriteStateKeyPair in ComponentDatas)
            {
                spriteStateKeyPair.Value.Enabled = spriteStateKeyPair.Value.SpriteComponent.Enabled;
            }
        }

        protected override void OnEntityComponentAdding(Entity entity, SpriteComponent spriteComponent, RenderSprite data)
        {
            renderSystem.RenderObjects.Add(data);
        }

        protected override void OnEntityComponentRemoved(Entity entity, SpriteComponent spriteComponent, RenderSprite data)
        {
            renderSystem.RenderObjects.Remove(data);
        }

        protected override RenderSprite GenerateComponentData(Entity entity, SpriteComponent spriteComponent)
        {
            return new RenderSprite
            {
                SpriteComponent = spriteComponent,
                TransformComponent = entity.Transform,
            };
        }

        protected override bool IsAssociatedDataValid(Entity entity, SpriteComponent spriteComponent, RenderSprite associatedData)
        {
            return
                spriteComponent == associatedData.SpriteComponent &&
                entity.Transform == associatedData.TransformComponent;
        }
    }
}