// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System;
using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
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

        /// <inheritdoc />
        protected internal override void OnSystemAdd()
        {
        }

        /// <inheritdoc />
        protected internal override void OnSystemRemove()
        {
            if (ActiveBackground != null)
            {
                VisibilityGroup.RenderObjects.Remove(ActiveBackground);
                ActiveBackground = null;
            }
        }

        /// <inheritdoc />
        protected override RenderBackground GenerateComponentData(Entity entity, BackgroundComponent component)
        {
            return new RenderBackground { RenderGroup = component.RenderGroup };
        }

        /// <inheritdoc />
        protected override bool IsAssociatedDataValid(Entity entity, BackgroundComponent component, RenderBackground associatedData)
        {
            return component.RenderGroup == associatedData.RenderGroup;
        }

        /// <inheritdoc />
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
                    renderBackground.RenderGroup = backgroundComponent.RenderGroup;
                    renderBackground.Rotation = Quaternion.RotationMatrix(backgroundComponent.Entity.Transform.WorldMatrix);

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
