// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using SiliconStudio.Core;
using SiliconStudio.Xenko.Engine;

namespace SiliconStudio.Xenko.Rendering.Lights
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
        {
            lights = new LightComponentCollection(DefaultLightCapacityCount);
            lightsCollected = new LightComponentCollection(DefaultLightCapacityCount);
        }

        /// <summary>
        /// Gets the active lights.
        /// </summary>
        /// <value>The lights.</value>
        public LightComponentCollection Lights => lightsCollected;

        protected override void OnEntityComponentAdding(Entity entity, LightComponent component, LightComponent state)
        {
            lights.Add(state);
        }

        protected override void OnEntityComponentRemoved(Entity entity, LightComponent component, LightComponent state)
        {
            lights.Remove(state);
        }

        public override void Draw(RenderContext context)
        {
            // 1) Clear the cache of current lights (without destroying collections but keeping previously allocated ones)
            lightsCollected.Clear();

            var colorSpace = context.GraphicsDevice.ColorSpace;

            // 2) Prepare lights to be dispatched to the correct light group
            for (int i = 0; i < lights.Count; i++)
            {
                var light = lights.Items[i];

                if (!light.Update(colorSpace))
                {
                    continue;
                }

                lightsCollected.Add(light);
            }
        }
    }
}
