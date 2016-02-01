// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using SiliconStudio.Core;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Rendering;

namespace SiliconStudio.Xenko.Rendering.Skyboxes
{
    /// <summary>
    /// A default entity processor for <see cref="SkyboxComponent"/>.
    /// </summary>
    public class SkyboxProcessor : EntityProcessor<SkyboxComponent, RenderSkybox>
    {
        private NextGenRenderSystem renderSystem;

        /// <summary>
        /// Gets the active skybox background.
        /// </summary>
        /// <value>The active skybox background.</value>
        public RenderSkybox ActiveSkybox { get; private set; }

        protected internal override void OnSystemAdd()
        {
            renderSystem = Services.GetSafeServiceAs<NextGenRenderSystem>();
        }

        protected override RenderSkybox GenerateComponentData(Entity entity, SkyboxComponent component)
        {
            return new RenderSkybox();
        }

        public override void Draw(RenderContext context)
        {
            var previousSkybox = ActiveSkybox;

            // Start by making it not visible
            foreach (var entityKeyPair in ComponentDatas)
            {
                var skyboxComponent = entityKeyPair.Key;
                var renderSkybox = entityKeyPair.Value;
                if (skyboxComponent.Enabled && skyboxComponent.Skybox != null)
                {
                    // Select the first active skybox
                    renderSkybox.Skybox = skyboxComponent.Skybox;
                    renderSkybox.Background = skyboxComponent.Background;
                    renderSkybox.Intensity = skyboxComponent.Intensity;

                    ActiveSkybox = renderSkybox;
                    break;
                }
            }

            if (ActiveSkybox != previousSkybox)
            {
                if (previousSkybox != null)
                    renderSystem.RenderObjects.Remove(previousSkybox);
                if (ActiveSkybox != null)
                    renderSystem.RenderObjects.Add(ActiveSkybox);
            }
        }
    }
}