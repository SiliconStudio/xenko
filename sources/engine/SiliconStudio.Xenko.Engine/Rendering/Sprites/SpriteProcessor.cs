// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Collections.Generic;
using SiliconStudio.Core;
using SiliconStudio.Xenko.Engine;

namespace SiliconStudio.Xenko.Rendering.Sprites
{
    /// <summary>
    /// The processor in charge of updating and drawing the entities having sprite components.
    /// </summary>
    internal class SpriteProcessor : EntityProcessor<SpriteComponent, SpriteProcessor.SpriteComponentState>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SpriteProcessor"/> class.
        /// </summary>
        public SpriteProcessor()
            : base(typeof(TransformComponent))
        {
            Sprites = new List<SpriteComponentState>();
        }

        protected internal override void OnSystemAdd()
        {
        }

        public List<SpriteComponentState> Sprites { get; private set; }

        public override void Draw(RenderContext gameTime)
        {
            Sprites.Clear();
            foreach (var spriteStateKeyPair in ComponentDatas)
            {
                if (spriteStateKeyPair.Value.SpriteComponent.Enabled)
                {
                    Sprites.Add(spriteStateKeyPair.Value);
                }
            }
        }

        protected override SpriteComponentState GenerateComponentData(Entity entity, SpriteComponent component)
        {
            return new SpriteComponentState
            {
                SpriteComponent = component,
                TransformComponent = entity.Transform,
            };
        }

        protected override bool IsAssociatedDataValid(Entity entity, SpriteComponent component, SpriteComponentState associatedData)
        {
            return
                component == associatedData.SpriteComponent &&
                entity.Transform == associatedData.TransformComponent;
        }

        public class SpriteComponentState
        {
            public SpriteComponent SpriteComponent;

            public TransformComponent TransformComponent;
        }
    }
}