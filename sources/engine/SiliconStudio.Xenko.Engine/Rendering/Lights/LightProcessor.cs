// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;
using SiliconStudio.Xenko.Engine;

namespace SiliconStudio.Xenko.Rendering.Lights
{
    /// <summary>
    /// Process <see cref="LightComponent"/> stored in an <see cref="EntityManager"/> by providing grouped lights per types/shadows.
    /// </summary>
    public class LightProcessor : EntityProcessor<LightComponent, LightComponent>
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
            base.OnEntityComponentAdding(entity, component, state);
            lights.Add(state);
        }

        protected override void OnEntityComponentRemoved(Entity entity, LightComponent component, LightComponent state)
        {
            base.OnEntityComponentRemoved(entity, component, state);
            lights.Remove(state);
        }

        protected override LightComponent GenerateComponentData(Entity entity, LightComponent component)
        {
            return component;
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
    