// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;

using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.EntityModel;
using SiliconStudio.Paradox.Games;

namespace SiliconStudio.Paradox.Effects.Lights
{
    /// <summary>
    /// Process <see cref="LightComponent"/> stored in an <see cref="EntityManager"/> by providing grouped lights per types/shadows.
    /// </summary>
    public class LightProcessor : EntityProcessor<LightComponent>
    {
        private const int DefaultLightCapacityCount = 512;

        private readonly LightComponentCollection lights;

        /// <summary>
        /// Initializes a new instance of the <see cref="LightProcessor"/> class.
        /// </summary>
        public LightProcessor()
            : base(new PropertyKey[] { LightComponent.Key })
        {
            lights = new LightComponentCollection(DefaultLightCapacityCount);

            // TODO: How should we handle RenderLayer? Should we precalculate layers here?
            ActiveDirectLights = new Dictionary<Type, LightComponentCollection>();
            ActiveEnvironmentLights = new Dictionary<Type, LightComponentCollection>();
            ActiveLightsWithShadow = new Dictionary<Type, LightComponentCollection>();
        }

        /// <summary>
        /// Gets all the lights.
        /// </summary>
        /// <value>The lights.</value>
        public LightComponentCollection Lights
        {
            get
            {
                return lights;
            }
        }

        /// <summary>
        /// Gets the lights with a shadow per light type.
        /// </summary>
        /// <value>The lights with shadow.</value>
        public Dictionary<Type, LightComponentCollection> ActiveLightsWithShadow { get; private set; }

        /// <summary>
        /// Gets the lights without shadow per light type.
        /// </summary>
        /// <value>The lights.</value>
        public Dictionary<Type, LightComponentCollection> ActiveDirectLights { get; private set; }

        /// <summary>
        /// Gets the lights without shadow per light type.
        /// </summary>
        /// <value>The lights.</value>
        public Dictionary<Type, LightComponentCollection> ActiveEnvironmentLights { get; private set; }

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

        private static void AddLightComponent(LightComponent light, Type newType, Dictionary<Type, LightComponentCollection> cache)
        {
            LightComponentCollection lightComponents;
            if (!cache.TryGetValue(newType, out lightComponents))
            {
                lightComponents = new LightComponentCollection(DefaultLightCapacityCount);
                cache.Add(newType, lightComponents);
            }

            lightComponents.Add(light);
        }

        public override void Draw(RenderContext context)
        {
            // Instead of clearing the types, we are clearing the underlying list (keeping the allocated space)
            foreach (var lightPair in ActiveDirectLights)
            {
                lightPair.Value.Clear();
            }

            foreach (var lightPair in ActiveEnvironmentLights)
            {
                lightPair.Value.Clear();
            }

            foreach (var lightPair in ActiveLightsWithShadow)
            {
                lightPair.Value.Clear();
            }

            // TODO: How should we handle RenderLayer? Should we precalculate layers here?
            for (int i = 0; i < lights.Count; i++)
            {
                UpdateLight(lights.Items[i]);
            }
        }

        private void UpdateLight(LightComponent light)
        {
            if (light.Type == null || !light.Enabled)
            {
                return;
            }
            var type = light.Type.GetType();
            var directLight = light.Type as IDirectLight;
            if (directLight != null)
            {
                AddLightComponent(light, type, directLight.Shadow != null && directLight.Shadow.Enabled ? ActiveLightsWithShadow : ActiveDirectLights);
            }
            else
            {
                AddLightComponent(light, type, ActiveEnvironmentLights);
            }

            Vector3 lightDirection;
            var lightDir = LightComponent.DefaultDirection;
            Vector3.TransformNormal(ref lightDir, ref light.Entity.Transform.WorldMatrix, out lightDirection);
            lightDirection.Normalize();
            light.Direction = lightDirection;
        }
    }
}
    