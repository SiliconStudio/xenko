// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;
using SiliconStudio.Paradox.EntityModel;
using SiliconStudio.Paradox.Games;

namespace SiliconStudio.Paradox.Effects.Skyboxes
{
    /// <summary>
    /// A default entity processor for <see cref="SkyboxComponent"/>.
    /// </summary>
    public class SkyboxProcessor : EntityProcessor<SkyboxComponent>
    {
        private readonly SkyboxComponentCollection skyboxes;
        private readonly SkyboxComponentCollection activeSkyboxLights;

        /// <summary>
        /// Initializes a new instance of the <see cref="SkyboxProcessor" /> class.
        /// </summary>
        public SkyboxProcessor()
            : base(new PropertyKey[] { SkyboxComponent.Key })
        {
            skyboxes = new SkyboxComponentCollection();
            activeSkyboxLights = new SkyboxComponentCollection();
        }

        /// <summary>
        /// Gets the active skybox background.
        /// </summary>
        /// <value>The active skybox background.</value>
        public SkyboxComponent ActiveSkyboxBackground { get; private set; }

        /// <summary>
        /// Gets the active skybox lights.
        /// </summary>
        /// <value>The active skybox lights.</value>
        public SkyboxComponentCollection ActiveSkyboxLights
        {
            get { return activeSkyboxLights; }
        }

        protected override void OnEntityAdding(Entity entity, SkyboxComponent data)
        {
            base.OnEntityAdding(entity, data);
            skyboxes.Add(data);
        }

        protected override void OnEntityRemoved(Entity entity, SkyboxComponent data)
        {
            base.OnEntityRemoved(entity, data);
            skyboxes.Remove(data);
        }

        /// <inheritdoc/>
        protected override SkyboxComponent GenerateAssociatedData(Entity entity)
        {
            return entity.Get(SkyboxComponent.Key);
        }

        public override void Draw(GameTime time)
        {
            ActiveSkyboxBackground = null;
            activeSkyboxLights.Clear();

            foreach (var skybox in skyboxes)
            {
                if (skybox.Enabled)
                {
                    if (ActiveSkyboxBackground == null && skybox.Background.Enabled)
                    {
                        ActiveSkyboxBackground = skybox;
                    }
                    if (skybox.Lighting.Enabled)
                    {
                        activeSkyboxLights.Add(skybox);
                    }
                }
            }
        }
    }
}