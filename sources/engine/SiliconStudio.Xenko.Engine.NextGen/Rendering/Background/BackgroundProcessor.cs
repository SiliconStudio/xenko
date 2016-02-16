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
    public class NextGenBackgroundProcessor : EntityProcessor<BackgroundComponent, RenderBackground>
    {
        private NextGenRenderSystem renderSystem;

        /// <summary>
        /// Gets the active background.
        /// </summary>
        /// <value>The active background.</value>
        public RenderBackground ActiveBackground { get; private set; }

        protected internal override void OnSystemAdd()
        {
            renderSystem = Services.GetSafeServiceAs<NextGenRenderSystem>();
        }

        protected override RenderBackground GenerateComponentData(Entity entity, BackgroundComponent component)
        {
            return new RenderBackground();
        }

        public override void Draw(RenderContext context)
        {
            var previousBackground = ActiveBackground;

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

                    ActiveBackground = renderBackground;
                    break;
                }
            }

            if (ActiveBackground != previousBackground)
            {
                if (previousBackground != null)
                    renderSystem.RenderObjects.Remove(previousBackground);
                if (ActiveBackground != null)
                    renderSystem.RenderObjects.Add(ActiveBackground);
            }
        }
    }
}