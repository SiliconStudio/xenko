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
    internal class SpriteRenderProcessor : EntityProcessor<SpriteComponent, RenderSprite>, IEntityComponentRenderProcessor
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
                var renderSprite = spriteStateKeyPair.Value;
                renderSprite.Enabled = renderSprite.SpriteComponent.Enabled;

                if (renderSprite.Enabled)
                {
                    // TODO GRAPHICS REFACTOR: Proper bounding box. Reuse calculations in sprite batch.
                    renderSprite.BoundingBox = new BoundingBoxExt(new Vector3(float.NegativeInfinity), new Vector3(float.PositiveInfinity));
                    renderSprite.RenderGroup = renderSprite.SpriteComponent.Entity.Group;
                }
            }
        }

        protected override void OnEntityComponentAdding(Entity entity, SpriteComponent spriteComponent, RenderSprite data)
        {
            VisibilityGroup.RenderObjects.Add(data);
        }

        protected override void OnEntityComponentRemoved(Entity entity, SpriteComponent spriteComponent, RenderSprite data)
        {
            VisibilityGroup.RenderObjects.Remove(data);
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