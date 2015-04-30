// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Collections.Generic;
using SiliconStudio.Core;
using SiliconStudio.Paradox.Rendering;

namespace SiliconStudio.Paradox.Engine.Processors
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
            : base(new PropertyKey[] { CameraComponent.Key })
        {
            Cameras = new List<CameraComponent>();
            Order = -10;
        }

        protected override CameraComponent GenerateAssociatedData(Entity entity)
        {
            return entity.Get<CameraComponent>();
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
            foreach (var matchingEntity in enabledEntities)
            {
                var camera = matchingEntity.Value;

                // Skip disabled model components, or model components without a proper model set
                if (!camera.Enabled)
                {
                    continue;
                }

                // Update the group in case it changed
                camera.Update();

                Cameras.Add(camera);
            }
        }
    }
}