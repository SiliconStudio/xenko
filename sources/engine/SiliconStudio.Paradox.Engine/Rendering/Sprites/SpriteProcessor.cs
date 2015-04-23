// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Collections.Generic;
using SiliconStudio.Core;
using SiliconStudio.Paradox.Engine;

namespace SiliconStudio.Paradox.Rendering.Sprites
{
    /// <summary>
    /// The processor in charge of updating and drawing the entities having sprite components.
    /// </summary>
    internal class SpriteProcessor : EntityProcessor<SpriteProcessor.SpriteComponentState>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SpriteProcessor"/> class.
        /// </summary>
        public SpriteProcessor()
            : base(new PropertyKey[] { SpriteComponent.Key, TransformComponent.Key })
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
            foreach (var spriteStateKeyPair in enabledEntities)
            {
                if (spriteStateKeyPair.Value.SpriteComponent.Enabled)
                {
                    Sprites.Add(spriteStateKeyPair.Value);
                }
            }
        }

        protected override SpriteComponentState GenerateAssociatedData(Entity entity)
        {
            return new SpriteComponentState
            {
                SpriteComponent = entity.Get(SpriteComponent.Key),
                TransformComponent = entity.Get(TransformComponent.Key),
            };
        }

        public class SpriteComponentState
        {
            public SpriteComponent SpriteComponent;

            public TransformComponent TransformComponent;
        }
    }
}