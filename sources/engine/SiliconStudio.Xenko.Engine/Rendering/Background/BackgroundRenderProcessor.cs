// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using SiliconStudio.Core;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Rendering;

namespace SiliconStudio.Xenko.Rendering.Background
{
    /// <summary>
    /// A default entity processor for <see cref="BackgroundComponent"/>.
    /// </summary>
    public class BackgroundRenderProcessor : EntityProcessor<BackgroundComponent, RenderBackground>, IEntityComponentRenderProcessor
    {
        public VisibilityGroup VisibilityGroup { get; set; }

        /// <summary>
        /// Gets the active background.
        /// </summary>
        /// <value>The active background.</value>
        public RenderBackground ActiveBackground { get; private set; }

        protected internal override void OnSystemAdd()
        {
        }

        protected internal override void OnSystemRemove()
        {
            if (ActiveBackground != null)
            {
                VisibilityGroup.RenderObjects.Remove(ActiveBackground);
                ActiveBackground = null;
            }
        }

        protected override RenderBackground GenerateComponentData(Entity entity, BackgroundComponent component)
        {
            return new RenderBackground();
        }

        public override void Draw(RenderContext context)
        {
            var previousBackground = ActiveBackground;
            ActiveBackground = null;

            // Start by making it not visible
            foreach (var entityKeyPair in ComponentDatas)
            {
                var backgroundComponent = entityKeyPair.Key;
                var renderBackground = entityKeyPair.Value;
                if (backgroundComponent.Enabled && backgroundComponent.Texture != null)
                {
                    // Select the first active background
                    renderBackground.Texture = backgroundComponent.Texture;
                    renderBackground.Intensity = backgroundComponent.Intensity;
                    renderBackground.RenderGroup = backgroundComponent.Entity.Group;

                    ActiveBackground = renderBackground;
                    break;
                }
            }

            if (ActiveBackground != previousBackground)
            {
                if (previousBackground != null)
                    VisibilityGroup.RenderObjects.Remove(previousBackground);
                if (ActiveBackground != null)
                    VisibilityGroup.RenderObjects.Add(ActiveBackground);
            }
        }
    }
}
