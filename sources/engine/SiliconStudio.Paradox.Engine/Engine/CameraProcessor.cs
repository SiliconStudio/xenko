// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Collections.Generic;

using SiliconStudio.Core;
using SiliconStudio.Paradox.Effects;
using SiliconStudio.Paradox.EntityModel;
using SiliconStudio.Paradox.Games;

namespace SiliconStudio.Paradox.Engine
{
    /// <summary>
    /// The processor for <see cref="CameraComponent"/>.
    /// </summary>
    public class CameraProcessor : EntityProcessor<CameraComponentState>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CameraProcessor"/> class.
        /// </summary>
        public CameraProcessor()
            : base(new PropertyKey[] { CameraComponent.Key })
        {
            Cameras = new List<CameraComponentState>();
        }

        protected override CameraComponentState GenerateAssociatedData(Entity entity)
        {
            return new CameraComponentState(entity.Get<CameraComponent>());
        }

        /// <summary>
        /// Gets the current models to render.
        /// </summary>
        /// <value>The current models to render.</value>
        public List<CameraComponentState> Cameras { get; private set; }

        public override void Draw(RenderContext context)
        {
            Cameras.Clear();

            // TODO: Find a better pluggabilty
            var globalCameras = context.GetAllCameras();

            // Collect models for this frame
            foreach (var matchingEntity in enabledEntities)
            {
                var state = matchingEntity.Value;

                // Skip disabled model components, or model components without a proper model set
                if (!state.CameraComponent.Enabled)
                {
                    continue;
                }

                // Update the group in case it changed
                state.Update();

                Cameras.Add(state);

                // Add the camera to the global state
                globalCameras[state.CameraComponent] = state;
            }
        }
    }
}