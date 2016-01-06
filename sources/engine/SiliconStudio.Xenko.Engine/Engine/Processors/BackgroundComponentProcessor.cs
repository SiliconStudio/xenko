// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Collections.Generic;
using SiliconStudio.Xenko.Rendering;
using SiliconStudio.Xenko.Rendering.Sprites;

namespace SiliconStudio.Xenko.Engine.Processors
{
    /// <summary>
    /// The processor in charge of updating and drawing the entities having background components.
    /// </summary>
    internal class BackgroundComponentProcessor : EntityProcessor<BackgroundComponent, BackgroundComponent>
    {
        public List<BackgroundComponent> Backgrounds { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SpriteProcessor"/> class.
        /// </summary>
        public BackgroundComponentProcessor()
        {
            Backgrounds = new List<BackgroundComponent>();
        }

        protected override BackgroundComponent GenerateAssociatedData(Entity entity, BackgroundComponent component)
        {
            return component;
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