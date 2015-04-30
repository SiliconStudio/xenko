// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Collections.Generic;
using SiliconStudio.Core;
using SiliconStudio.Paradox.Rendering;
using SiliconStudio.Paradox.Rendering.Sprites;

namespace SiliconStudio.Paradox.Engine.Processors
{
    /// <summary>
    /// The processor in charge of updating and drawing the entities having background components.
    /// </summary>
    internal class BackgroundComponentProcessor : EntityProcessor<BackgroundComponent>
    {
        public List<BackgroundComponent> Backgrounds { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SpriteProcessor"/> class.
        /// </summary>
        public BackgroundComponentProcessor()
            : base(new PropertyKey[] { BackgroundComponent.Key })
        {
            Backgrounds = new List<BackgroundComponent>();
        }

        protected override BackgroundComponent GenerateAssociatedData(Entity entity)
        {
            return entity.Get<BackgroundComponent>();
        }

        public override void Draw(RenderContext gameTime)
        {
            Backgrounds.Clear();
            foreach (var background in enabledEntities.Values)
            {
                if (background.Enabled)
                {
                    Backgrounds.Add(background);
                }
            }
        }
    }
}