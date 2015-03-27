// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;

using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.EntityModel;

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
            ActiveDirectLights = new Dictionary<Type, LightComponentCollectionGroup>();
            ActiveEnvironmentLights = new Dictionary<Type, LightComponentCollectionGroup>();
            ActiveDirectLightsWithShadow = new Dictionary<Type, LightComponentCollectionGroup>();
        }

        /// <summary>
        /// Gets the lights with a shadow per light type.
        /// </summary>
        /// <value>The lights with shadow.</value>
        public Dictionary<Type, LightComponentCollectionGroup> ActiveDirectLightsWithShadow { get; private set; }

        /// <summary>
        /// Gets the lights without shadow per light type.
        /// </summary>
        /// <value>The lights.</value>
        public Dictionary<Type, LightComponentCollectionGroup> ActiveDirectLights { get; private set; }

        /// <summary>
        /// Gets the lights without shadow per light type.
        /// </summary>
        /// <value>The lights.</value>
        public Dictionary<Type, LightComponentCollectionGroup> ActiveEnvironmentLights { get; private set; }

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
            // 1) Clear the cache of current lights
            ClearCache();

            // 2) Prepare lights to be dispatched to the correct light group
            for (int i = 0; i < lights.Count; i++)
            {
                PrepareLight(lights.Items[i]);
            }

            // 3) Allocate collection based on prepass
            AllocateCollections();

            // 4) Collect light to the correct light group
            for (int i = 0; i < lights.Count; i++)
            {
                CollectLight(lights.Items[i]);
            }
        }

        private void AllocateCollections()
        {
            foreach (var lightPair in ActiveDirectLights)
            {
                lightPair.Value.AllocateCollections();
            }

            foreach (var lightPair in ActiveEnvironmentLights)
            {
                lightPair.Value.AllocateCollections();
            }

            foreach (var lightPair in ActiveDirectLightsWithShadow)
            {
                lightPair.Value.AllocateCollections();
            }
        }

        private void ClearCache()
        {
            foreach (var lightPair in ActiveDirectLights)
            {
                lightPair.Value.Clear();
            }

            foreach (var lightPair in ActiveEnvironmentLights)
            {
                lightPair.Value.Clear();
            }

            foreach (var lightPair in ActiveDirectLightsWithShadow)
            {
                lightPair.Value.Clear();
            }
        }

        private void CollectLight(LightComponent light)
        {
            if (light.Type == null || !light.Enabled)
            {
                return;
            }

            var lightGroup = GetLightGroup(light);
            lightGroup.AddLight(light);
        }

        private void PrepareLight(LightComponent light)
        {
            if (light.Type == null || !light.Enabled)
            {
                return;
            }
            var lightGroup = GetLightGroup(light);
            lightGroup.PrepareLight(light);

            // Update direction for light
            Vector3 lightDirection;
            var lightDir = LightComponent.DefaultDirection;
            Vector3.TransformNormal(ref lightDir, ref light.Entity.Transform.WorldMatrix, out lightDirection);
            lightDirection.Normalize();
            light.Direction = lightDirection;
        }

        private LightComponentCollectionGroup GetLightGroup(LightComponent light)
        {
            var type = light.Type.GetType();
            var directLight = light.Type as IDirectLight;
            Dictionary<Type, LightComponentCollectionGroup> cache;
            if (directLight != null)
            {
                cache = directLight.Shadow != null && directLight.Shadow.Enabled ? ActiveDirectLightsWithShadow : ActiveDirectLights;
            }
            else
            {
                cache = ActiveEnvironmentLights;
            }

            LightComponentCollectionGroup lightGroup;
            if (!cache.TryGetValue(type, out lightGroup))
            {
                lightGroup = new LightComponentCollectionGroup();
                cache.Add(type, lightGroup);
            }
            return lightGroup;
        }
    }
}
    