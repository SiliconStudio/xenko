// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;
using SiliconStudio.Paradox.Engine;
using SiliconStudio.Paradox.Rendering;

namespace SiliconStudio.Paradox.Rendering.Skyboxes
{
    /// <summary>
    /// A default entity processor for <see cref="SkyboxComponent"/>.
    /// </summary>
    public class SkyboxProcessor : EntityProcessor<SkyboxComponent>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SkyboxProcessor" /> class.
        /// </summary>
        public SkyboxProcessor()
            : base(new PropertyKey[] { SkyboxComponent.Key })
        {
        }

        /// <summary>
        /// Gets the active skybox background.
        /// </summary>
        /// <value>The active skybox background.</value>
        public SkyboxComponent ActiveSkyboxBackground { get; private set; }

        /// <inheritdoc/>
        protected override SkyboxComponent GenerateAssociatedData(Entity entity)
        {
            return entity.Get(SkyboxComponent.Key);
        }

        public override void Draw(RenderContext context)
        {
            ActiveSkyboxBackground = null;

            foreach (var entityKeyPair in matchingEntities)
            {
                var skybox = entityKeyPair.Value;
                if (skybox.Enabled && skybox.Skybox != null)
                {
                    // Select the first active skybox
                    ActiveSkyboxBackground = skybox;
                    break;
                }
            }
        }
    }
}