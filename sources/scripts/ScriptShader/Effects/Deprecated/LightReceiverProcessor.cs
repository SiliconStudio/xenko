// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using SiliconStudio.Xenko.DataModel;
using SiliconStudio.Xenko.Rendering;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Games;
using SiliconStudio.Core;
using SiliconStudio.Core.Collections;

namespace ScriptShader.Effects
{
    public class LightReceiverProcessor : EntityProcessor<LightReceiverProcessor.AssociatedData>
    {
        private HashSet<ShadowMap> activeShadowMaps = new HashSet<ShadowMap>();
        private LightProcessor lightProcessor;

        public LightReceiverProcessor(LightProcessor lightProcessor)
            : base(new PropertyKey[] { MeshComponent.Key })
        {
            this.lightProcessor = lightProcessor;
            var globalLights = new TrackingCollection<LightComponent>();
            globalLights.CollectionChanged += globalLights_CollectionChanged;
            GlobalLights = globalLights;
        }

        void globalLights_CollectionChanged(object sender, TrackingCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
                InternalAddEntity(((LightComponent)e.Item).Entity);
            else if (e.Action == NotifyCollectionChangedAction.Remove)
                InternalRemoveEntity(((LightComponent)e.Item).Entity);
        }

        public IList<LightComponent> GlobalLights { get; private set; }

        protected override AssociatedData GenerateAssociatedData(Entity entity)
        {
            return new AssociatedData { LightReceiverComponent = entity.GetOrCreate(LightReceiverComponent.Key), MeshComponent = entity.Get(MeshComponent.Key) };
        }

        protected override void OnSystemAdd()
        {
            base.OnSystemAdd();

            lightProcessor.LightComponentAdded += LightProcessorLightComponentAdded;
            lightProcessor.LightComponentRemoved += LightProcessorLightComponentRemoved;
        }

        protected override void OnSystemRemove()
        {
            lightProcessor.LightComponentAdded -= LightProcessorLightComponentAdded;
            lightProcessor.LightComponentRemoved -= LightProcessorLightComponentRemoved;
        }
        
        private void LightProcessorLightComponentAdded(object sender, LightComponent e)
        {
            if (!GlobalLights.Contains(e))
                GlobalLights.Add(e);
            bool globalLightUpdated = (GlobalLights.Contains(e));
            foreach (var matchingEntity in matchingEntities)
            {
                if (globalLightUpdated
                    || matchingEntity.Value.LightReceiverComponent.LightComponents.Contains(e))
                {
                    matchingEntity.Value.LightingPermutationUpdated = true;
                }
            }
        }

        private void LightProcessorLightComponentRemoved(object sender, LightComponent e)
        {
            GlobalLights.Remove(e);
            foreach (var matchingEntity in matchingEntities)
            {
                matchingEntity.Value.LightingPermutationUpdated = true;
            }
        }

        protected override void OnEntityAdded(Entity entity, AssociatedData data)
        {
            base.OnEntityAdded(entity, data);

            data.LightComponentsChanged = (sender, e) =>
                {
                    data.LightingPermutationUpdated = true;
                };

            data.LightingPermutationUpdated = true;
            ((TrackingCollection<LightComponent>)data.LightReceiverComponent.LightComponents).CollectionChanged += data.LightComponentsChanged;
        }

        protected override void OnEntityRemoved(Entity entity, AssociatedData data)
        {
            base.OnEntityRemoved(entity, data);

            ((TrackingCollection<LightComponent>)data.LightReceiverComponent.LightComponents).CollectionChanged -= data.LightComponentsChanged;
        }

        public override void Update()
        {
            foreach (var matchingEntity in enabledEntities)
            {
                if (matchingEntity.Value.LightingPermutationUpdated)
                {
                    matchingEntity.Value.LightingPermutationUpdated = false;

                    var lightComponents = matchingEntity.Value.LightReceiverComponent.LightComponents.Count > 0
                                              ? GlobalLights.Concat(matchingEntity.Value.LightReceiverComponent.LightComponents)
                                              : GlobalLights;

                    int shadowMapCount = 0;
                    int normalLightCount = 0;

                    // Count lights
                    foreach (var lightComponent in lightComponents)
                    {
                        if (lightComponent.Tags.ContainsKey(LightProcessor.ShadowMapKey))
                            shadowMapCount++;
                        else if (!lightComponent.Deferred)
                            normalLightCount++;
                    }

                    var shadowMapPermutationArray = new ShadowMapPermutationArray { ShadowMaps = new ShadowMapPermutation[shadowMapCount] };
                    var lightBindings = new LightBinding[normalLightCount];

                    // Collect lights
                    int lightingPermutationIndex = 0;
                    int shadowMapPermutationIndex = 0;
                    foreach (var lightComponent in lightComponents)
                    {
                        if (lightComponent.ShadowMap)
                        {
                            var currentLight = lightComponent.Get(LightProcessor.ShadowMapKey);
                            if (activeShadowMaps.Add(currentLight.ShadowMap))
                            {
                                //lightingPlugin.AddShadowMap(currentLight.ShadowMap);
                            }
                            shadowMapPermutationArray.ShadowMaps[shadowMapPermutationIndex++] = currentLight;
                        }
                        else if (!lightComponent.Deferred)
                        {
                            lightBindings[lightingPermutationIndex++] = new LightBinding(lightComponent.Get(LightProcessor.LightKey));
                        }
                    }

                    bool forcePerPixelLighting = matchingEntity.Value.LightReceiverComponent.Get(LightReceiverComponent.ForcePerPixelLighting);
                    if (forcePerPixelLighting)
                    {
                        for (int index = 0; index < lightBindings.Length; index++)
                        {
                            lightBindings[index].LightShaderType = LightShaderType.DiffuseSpecularPixel;
                        }
                    }

                    var lightingPermutation = new LightingPermutation(lightBindings);

                    // Update permutations
                    matchingEntity.Value.MeshComponent.Model.Permutations.Set(LightingPermutation.Key, lightingPermutation);
                    matchingEntity.Value.MeshComponent.Model.Permutations.Set(ShadowMapPermutationArray.Key, shadowMapPermutationArray);
                }
            }
        }

        public class AssociatedData
        {
            public bool LightingPermutationUpdated;
            public EventHandler<TrackingCollectionChangedEventArgs> LightComponentsChanged;
            public LightReceiverComponent LightReceiverComponent;
            public MeshComponent MeshComponent;
        }
    }
}
