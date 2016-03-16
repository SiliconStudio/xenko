using System.Collections.Generic;
using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Rendering;

namespace SiliconStudio.Xenko.Rendering.Sprites
{
    /// <summary>
    /// The processor in charge of updating and drawing the entities having sprite components.
    /// </summary>
    internal class SpriteRenderProcessor : EntityProcessor<SpriteComponent, SpriteRenderProcessor.SpriteInfo>, IEntityComponentRenderProcessor
    {
        public VisibilityGroup VisibilityGroup { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SpriteRenderProcessor"/> class.
        /// </summary>
        public SpriteRenderProcessor()
            : base(typeof(TransformComponent))
        {
        }

        public override void Draw(RenderContext gameTime)
        {
            foreach (var spriteStateKeyPair in ComponentDatas)
            {
                var renderSprite = spriteStateKeyPair.Value.RenderSprite;

                // TODO Should we allow adding RenderSprite without a CurrentSprite instead? (if yes, need some improvement in RenderSystem)
                if (spriteStateKeyPair.Value.Active != (spriteStateKeyPair.Key.CurrentSprite != null))
                {
                    spriteStateKeyPair.Value.Active = (spriteStateKeyPair.Key.CurrentSprite != null);
                    if (spriteStateKeyPair.Value.Active)
                        VisibilityGroup.RenderObjects.Add(renderSprite);
                    else
                        VisibilityGroup.RenderObjects.Remove(renderSprite);
                }

                renderSprite.Enabled = renderSprite.SpriteComponent.Enabled;

                if (renderSprite.Enabled)
                {
                    var transform = renderSprite.TransformComponent;

                    // TODO GRAPHICS REFACTOR: Proper bounding box. Reuse calculations in sprite batch.
                    // For now we only set a center for sorting, but no extent (which disable culling)
                    renderSprite.BoundingBox = new BoundingBoxExt { Center = transform.WorldMatrix.TranslationVector };
                    renderSprite.RenderGroup = renderSprite.SpriteComponent.Entity.Group;
                }
            }
        }

        protected override void OnEntityComponentRemoved(Entity entity, SpriteComponent component, SpriteInfo data)
        {
            VisibilityGroup.RenderObjects.Remove(data.RenderSprite);
        }

        protected override SpriteInfo GenerateComponentData(Entity entity, SpriteComponent spriteComponent)
        {
            return new SpriteInfo
            {
                RenderSprite = new RenderSprite
                {
                    SpriteComponent = spriteComponent,
                    TransformComponent = entity.Transform,
                }
            };
        }

        protected override bool IsAssociatedDataValid(Entity entity, SpriteComponent spriteComponent, SpriteInfo associatedData)
        {
            return
                spriteComponent == associatedData.RenderSprite.SpriteComponent &&
                entity.Transform == associatedData.RenderSprite.TransformComponent;
        }

        public class SpriteInfo
        {
            public bool Active;
            public RenderSprite RenderSprite;
        }
    }
}