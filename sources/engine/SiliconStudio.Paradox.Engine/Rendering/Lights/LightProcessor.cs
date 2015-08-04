// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;
using SiliconStudio.Paradox.Engine;

namespace SiliconStudio.Paradox.Rendering.Lights
{
    /// <summary>
    /// Process <see cref="LightComponent"/> stored in an <see cref="EntityManager"/> by providing grouped lights per types/shadows.
    /// </summary>
    public class LightProcessor : EntityProcessor<LightComponent>
    {
        private const int DefaultLightCapacityCount = 512;

        private readonly LightComponentCollection lightsCollected;

        private readonly LightComponentCollection lights;

        /// <summary>
        /// Initializes a new instance of the <see cref="LightProcessor"/> class.
        /// </summary>
        public LightProcessor()
            : base(new PropertyKey[] { LightComponent.Key })
        {
            lights = new LightComponentCollection(DefaultLightCapacityCount);
            lightsCollected = new LightComponentCollection(DefaultLightCapacityCount);
        }

        /// <summary>
        /// Gets the active lights.
        /// </summary>
        /// <value>The lights.</value>
        public LightComponentCollection Lights
        {
            get
            {
                return lightsCollected;
            }
        }

        protected override void OnEntityAdding(Entity entity, LightComponent state)
        {
            base.OnEntityAdding(entity, state);
            lights.Add(state);
        }

        protected override void OnEntityRemoved(Entity entity, LightComponent state)
        {
            base.OnEntityRemoved(entity, state);
            lights.Remove(state);
        }

        protected override LightComponent GenerateAssociatedData(Entity entity)
        {
            return entity.Get<LightComponent>();
        }

        public override void Draw(RenderContext context)
        {
            // 1) Clear the cache of current lights (without destroying collections but keeping previously allocated ones)
            lightsCollected.Clear();

            // 2) Prepare lights to be dispatched to the correct light group
            for (int i = 0; i < lights.Count; i++)
            {
                var light = lights.Items[i];

                if (!light.Update())
                {
                    continue;
                }

                lightsCollected.Add(light);
            }
        }
    }
}
    