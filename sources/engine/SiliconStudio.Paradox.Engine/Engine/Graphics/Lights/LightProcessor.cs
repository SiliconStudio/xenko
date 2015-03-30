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

        private readonly List<LightComponent> lightsCollected;

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
            // 1) Clear the cache of current lights (without destroying collections but keeping previously allocated ones)
            ClearCache();

            // 2) Prepare lights to be dispatched to the correct light group
            for (int i = 0; i < lights.Count; i++)
            {
                PrepareLight(lights.Items[i]);
            }

            // 3) Allocate collection based on their culling mask
            AllocateCollectionsPerGroupOfCullingMask();

            // 4) Collect lights to the correct light collection group
            foreach (var light in lightsCollected)
            {
                light.Group.AddLight(light);
            }
        }

        private void AllocateCollectionsPerGroupOfCullingMask()
        {
            AllocateCollectionsPerGroupOfCullingMask(ActiveDirectLights);
            AllocateCollectionsPerGroupOfCullingMask(ActiveDirectLightsWithShadow);
            AllocateCollectionsPerGroupOfCullingMask(ActiveEnvironmentLights);
        }

        private static void AllocateCollectionsPerGroupOfCullingMask(Dictionary<Type, LightComponentCollectionGroup> lights)
        {
            foreach (var lightPair in lights)
            {
                lightPair.Value.AllocateCollectionsPerGroupOfCullingMask();
            }
        }

        private void ClearCache()
        {
            lightsCollected.Clear();

            ClearCache(ActiveDirectLights);
            ClearCache(ActiveDirectLightsWithShadow);
            ClearCache(ActiveEnvironmentLights);
        }

        private static void ClearCache(Dictionary<Type, LightComponentCollectionGroup> lights)
        {
            foreach (var lightPair in lights)
            {
                lightPair.Value.Clear();
            }
        }

        private void PrepareLight(LightComponent light)
        {
            if (light.Type == null || !light.Enabled)
            {
                return;
            }
            var lightGroup = GetLightGroup(light);
            lightGroup.PrepareLight(light);
            light.Group = lightGroup;

            // Update direction for light
            Vector3 lightDirection;
            var lightDir = LightComponent.DefaultDirection;
            Vector3.TransformNormal(ref lightDir, ref light.Entity.Transform.WorldMatrix, out lightDirection);
            lightDirection.Normalize();
            light.Direction = lightDirection;

            lightsCollected.Add(light);
        }

        private LightComponentCollectionGroup GetLightGroup(LightComponent light)
        {
            var directLight = light.Type as IDirectLight;
            var cache = (directLight != null)
                ? directLight.Shadow != null && directLight.Shadow.Enabled ? ActiveDirectLightsWithShadow : ActiveDirectLights
                : ActiveEnvironmentLights;

            LightComponentCollectionGroup lightGroup;
            var type = light.Type.GetType();
            if (!cache.TryGetValue(type, out lightGroup))
            {
                lightGroup = new LightComponentCollectionGroup();
                cache.Add(type, lightGroup);
            }
            return lightGroup;
        }
    }
}
    