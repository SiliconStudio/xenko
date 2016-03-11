// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Rendering;

namespace SiliconStudio.Xenko.Rendering.Skyboxes
{
    /// <summary>
    /// A default entity processor for <see cref="SkyboxComponent"/>.
    /// </summary>
    public class SkyboxRenderProcessor : EntityProcessor<SkyboxComponent, RenderSkybox>, IEntityComponentRenderProcessor
    {
        public VisibilityGroup VisibilityGroup { get; set; }

        /// <summary>
        /// Gets the active skybox background.
        /// </summary>
        /// <value>The active skybox background.</value>
        public RenderSkybox ActiveSkybox { get; private set; }

        protected internal override void OnSystemRemove()
        {
            if (ActiveSkybox != null)
            {
                VisibilityGroup.RenderObjects.Remove(ActiveSkybox);
                ActiveSkybox = null;
            }
        }

        protected override RenderSkybox GenerateComponentData(Entity entity, SkyboxComponent component)
        {
            return new RenderSkybox();
        }

        public override void Draw(RenderContext context)
        {
            var previousSkybox = ActiveSkybox;
            ActiveSkybox = null;

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
                    renderSkybox.Rotation = skyboxComponent.Entity.Transform.Rotation;

                    renderSkybox.RenderGroup = skyboxComponent.Entity.Group;

                    ActiveSkybox = renderSkybox;
                    break;
                }
            }

            if (ActiveSkybox != previousSkybox)
            {
                if (previousSkybox != null)
                    VisibilityGroup.RenderObjects.Remove(previousSkybox);
                if (ActiveSkybox != null)
                    VisibilityGroup.RenderObjects.Add(ActiveSkybox);
            }
        }
    }
}