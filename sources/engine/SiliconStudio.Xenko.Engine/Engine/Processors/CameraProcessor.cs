// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Collections.Generic;
using SiliconStudio.Core;
using SiliconStudio.Xenko.Rendering;

namespace SiliconStudio.Xenko.Engine.Processors
{
    /// <summary>
    /// The processor for <see cref="CameraComponent"/>.
    /// </summary>
    public class CameraProcessor : EntityProcessor<CameraComponent>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CameraProcessor"/> class.
        /// </summary>
        public CameraProcessor()
        {
            Cameras = new List<CameraComponent>();
            Order = -10;
        }

        protected override CameraComponent GenerateComponentData(Entity entity, CameraComponent component)
        {
            return component;
        }

        /// <summary>
        /// Gets the current models to render.
        /// </summary>
        /// <value>The current models to render.</value>
        public List<CameraComponent> Cameras { get; private set; }

        public override void Draw(RenderContext context)
        {
            Cameras.Clear();

            // Collect models for this frame
            foreach (var matchingCamera in ComponentDatas)
            {
                var camera = matchingCamera.Value;

                // Skip disabled model components, or model components without a proper model set
                if (!camera.Enabled)
                {
                    continue;
                }

                // In case the camera has a custom aspect ratio, we can update it here
                // otherwise it is screen-dependent and we can only update it in the CameraComponentRenderer.
                if (camera.UseCustomAspectRatio)
                {
                    camera.Update();
                }

                Cameras.Add(camera);
            }
        }
    }
}