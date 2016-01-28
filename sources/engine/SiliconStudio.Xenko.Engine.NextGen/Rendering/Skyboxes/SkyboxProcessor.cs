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
    public class SkyboxProcessor : EntityProcessor<SkyboxComponent>
    {
        private readonly NextGenRenderSystem renderSystem;

        /// <summary>
        /// Initializes a new instance of the <see cref="SkyboxProcessor" /> class.
        /// </summary>
        public SkyboxProcessor(NextGenRenderSystem renderSystem)
        {
            this.renderSystem = renderSystem;
        }

        /// <summary>
        /// Gets the active skybox background.
        /// </summary>
        /// <value>The active skybox background.</value>
        public RenderSkybox ActiveSkybox { get; } = new RenderSkybox();

        protected override SkyboxComponent GenerateComponentData(Entity entity, SkyboxComponent component)
        {
            return component;
        }

        protected internal override void OnSystemAdd()
        {
            renderSystem.RenderObjects.Add(ActiveSkybox);
        }

        protected internal override void OnSystemRemove()
        {
            renderSystem.RenderObjects.Remove(ActiveSkybox);
        }

        public override void Draw(RenderContext context)
        {
            // Start by making it not visible
            ActiveSkybox.Visible = false;

            foreach (var entityKeyPair in ComponentDatas)
            {
                var skybox = entityKeyPair.Value;
                if (skybox.Enabled && skybox.Skybox != null)
                {
                    // Select the first active skybox
                    ActiveSkybox.Visible = true;
                    ActiveSkybox.Skybox = skybox.Skybox;
                    ActiveSkybox.Background = skybox.Background;
                    ActiveSkybox.Intensity = skybox.Intensity;
                    break;
                }
            }
        }
    }
}