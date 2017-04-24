// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;
using System.Collections.Generic;
using System.Linq;

using SiliconStudio.Xenko;
using SiliconStudio.Xenko.DataModel;
using SiliconStudio.Xenko.Rendering;
using SiliconStudio.Xenko.Rendering;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Games;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Core;
using SiliconStudio.Core.Collections;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Core.Serialization.Assets;
using SiliconStudio.Xenko.Shaders;

namespace ScriptShader.Effects
{
    /// <summary>Light processor.</summary>
    public class LightProcessor : EntityProcessor<LightProcessor.AssociatedData>
    {
        public static readonly PropertyKey<Light> LightKey = new PropertyKey<Light>("Light", typeof(LightProcessor));
        public static readonly PropertyKey<ShadowMapPermutation> ShadowMapKey = new PropertyKey<ShadowMapPermutation>("ShadowMap", typeof(LightProcessor));
        private IEffectSystemOld effectSystemOld;
        private IRenderSystem renderSystem;
        private IGraphicsDeviceService graphicsDeviceService;
        private IAssetManager assetManager;

        private EffectOld lightEffect;
        private EffectOld lightIconEffect;
        private LightingPlugin lightingPlugin;
        private Texture2D lightBulbTexture;
        private RenderTargetsPlugin renderTargetsPlugin;

        private bool enableIcons;

        private IRenderPassEnumerator renderPassEnumerator;

        public event Action<LightProcessor, LightComponent> LightComponentAdded;
        public event Action<LightProcessor, LightComponent> LightComponentRemoved;

        public LightProcessor(LightingPlugin lightingPlugin, RenderTargetsPlugin renderTargetsPlugin, EffectOld lightEffect, bool enableIcons)
            : base(new PropertyKey[] { LightComponent.Key, TransformationComponent.Key })
        {
            this.enableIcons = enableIcons;
            this.lightingPlugin = lightingPlugin;
            this.lightEffect = lightEffect;
            this.renderTargetsPlugin = renderTargetsPlugin;

            renderPassEnumerator = new RenderPassListEnumerator();
        }

        protected override void OnSystemAdd()
        {
            this.effectSystemOld = Services.GetSafeServiceAs<IEffectSystemOld>();
            renderSystem = Services.GetSafeServiceAs<IRenderSystem>();
            graphicsDeviceService = Services.GetSafeServiceAs<IGraphicsDeviceService>();
            assetManager = Services.GetSafeServiceAs<IAssetManager>();

            renderSystem.RenderPassEnumerators.Add(renderPassEnumerator);

            if (enableIcons)
                InitializeIconsRenderer();
        }

        private void InitializeIconsRenderer()
        {
            var graphicsDevice = graphicsDeviceService.GraphicsDevice;

            var lightIconEffectBuilder = this.effectSystemOld.BuildEffect("LightIcon")
                .Using(new BasicShaderPlugin(
                           new ShaderMixinSource
                               {
                                   Mixins = new List<ShaderClassSource>()
                                   {
                                   "ShaderBase",
                                   "TransformationZero",
                                   "TransformationWVP",
                                   "EditorIcon",
                                   },
                                   Compositions =  new Dictionary<string, ShaderSource>()
                                   { {"color", new ShaderClassSource("ComputeColorTexture", TexturingKeys.Texture0.Name, "TEXCOORD")}}
                               }) { RenderPassPlugin = renderTargetsPlugin })
                .Using(new StateShaderPlugin { UseBlendState = true, UseDepthStencilState = true, RenderPassPlugin = renderTargetsPlugin });
            lightIconEffectBuilder.PickingPassMainPlugin = renderTargetsPlugin;
            lightIconEffect = lightIconEffectBuilder;

            lightIconEffect.Parameters.Set(EffectPlugin.BlendStateKey, BlendState.New(graphicsDevice, new BlendStateDescription(Blend.SourceAlpha, Blend.InverseSourceAlpha)));
            lightIconEffect.Parameters.Set(EffectPlugin.DepthStencilStateKey, graphicsDevice.DepthStencilStates.Default);

            throw new NotImplementedException();
            //lightIconEffect.Passes[0].UpdatePasses += (RenderPass currentRenderPass, ref FastList<RenderPass> currentPasses) =>
            //    {
            //        if (currentRenderPass.Passes != null)
            //        {
            //            var meshComparer = new MeshComparer(renderTargetsPlugin.Parameters);
            //            currentRenderPass.Passes.Sort(meshComparer);
            //        }
            //    };

            // Generates a quad for post effect rendering (should be utility function)
            var vertices = new[]
                {
                    0.0f, 1.0f,
                    1.0f, 1.0f,
                    0.0f, 0.0f,
                    1.0f, 0.0f,
                };

            lightBulbTexture = (Texture2D)assetManager.Load<Texture>("/global_data/editor/light_bulb.png");
        }

        protected override AssociatedData GenerateAssociatedData(Entity entity)
        {
            return new AssociatedData
                {
                    LightComponent = entity.Get(LightComponent.Key),
                    TransformationComponent = entity.Transformation,
                };
        }

        public override void Update()
        {
            foreach (var matchingEntity in enabledEntities)
            {
                var transformationComponent = matchingEntity.Value.TransformationComponent;
                var lightComponent = matchingEntity.Value.LightComponent;
                var effectMesh = matchingEntity.Value.EffectMesh;

                // Check if entity has been updated and consequently need to be readded.
                if (lightComponent.ShadowMap != matchingEntity.Value.CurrentShadowMap
                    || lightComponent.Type != matchingEntity.Value.CurrentType)
                    EntityReadd(matchingEntity.Key);

                if (lightComponent.Deferred)
                {
                    // For now, only point light are supported
                    if (effectMesh == null)
                        continue;

                    effectMesh.Parameters.Set(LightKeys.LightColor, lightComponent.Color);
                    effectMesh.Parameters.Set(LightKeys.LightIntensity, lightComponent.Intensity);
                    effectMesh.Parameters.Set(LightKeys.LightRadius, lightComponent.DecayStart);
                    effectMesh.Parameters.Set(LightKeys.LightPosition, Vector3.TransformCoordinate(Vector3.Zero, transformationComponent.WorldMatrix));
                }
                else
                {
                    if (lightComponent.Type == LightType.Directional)
                    {
                        var directionalLight = (DirectionalLight)matchingEntity.Value.Light;
                        directionalLight.LightColor = lightComponent.Color * lightComponent.Intensity;
                        // Light direction should include transformation to be in world space.
                        directionalLight.LightDirection = Vector3.Normalize(Vector3.TransformNormal(lightComponent.LightDirection, transformationComponent.WorldMatrix));
                        directionalLight.LightShaderType = lightComponent.LightShaderType;
                    }
                }

                if (matchingEntity.Value.IconMesh != null)
                {
                    matchingEntity.Value.IconMesh.Parameters.Set(TransformationKeys.World, transformationComponent.WorldMatrix);
                    matchingEntity.Value.IconMesh.Parameters.Set(MaterialTexturingKeys.DiffuseTexture0, lightBulbTexture);
                }
            }
        }

        protected override void OnEntityAdded(Entity entity, AssociatedData associatedData)
        {
            if (associatedData.LightComponent.Deferred)
            {
                associatedData.EffectMesh = new EffectMesh(lightEffect);
                associatedData.EffectMesh.AddReference();
                renderSystem.GlobalMeshes.AddMesh(associatedData.EffectMesh);
            }
            else
            {
                if (associatedData.LightComponent.Type == LightType.Directional)
                {
                    associatedData.Light = new DirectionalLight();
                }
                else
                {
                    throw new NotImplementedException();
                }

                associatedData.LightComponent.Set(LightKey, associatedData.Light);
            }

            associatedData.CurrentShadowMap = associatedData.LightComponent.ShadowMap;
            associatedData.CurrentType = associatedData.LightComponent.Type;

            ShadowMapPermutation shadowMap = null;
            if (associatedData.LightComponent.ShadowMap)
            {
                shadowMap = new ShadowMap((DirectionalLight)associatedData.Light) { Level = CascadeShadowMapLevel.X2, ShadowMapSize = 1024, ShadowDistance = associatedData.LightComponent.DecayStart }
                    .SetFilter(sm => new ShadowMapFilterVsm(sm));

                lightingPlugin.AddShadowMap(shadowMap.ShadowMap);

                associatedData.ShadowMapPermutation = shadowMap;
                associatedData.LightComponent.Set(ShadowMapKey, associatedData.ShadowMapPermutation);
            }

            if (enableIcons)
            {
                throw new NotImplementedException();
                associatedData.IconMesh = new EffectMesh(lightIconEffect /*, iconMeshData, null*/);
                associatedData.IconMesh.AddReference();
                renderPassEnumerator.AddMesh(associatedData.IconMesh);
                associatedData.IconMesh.Tags.Set(PickingPlugin.AssociatedEntity, entity);
            }

            if (LightComponentAdded != null)
                LightComponentAdded(this, associatedData.LightComponent);
        }

        protected override void OnEntityRemoved(Entity entity, AssociatedData associatedData)
        {
            if (LightComponentRemoved != null)
                LightComponentRemoved(this, associatedData.LightComponent);

            if (associatedData.EffectMesh != null)
            {
                renderSystem.GlobalMeshes.RemoveMesh(associatedData.EffectMesh);
                associatedData.EffectMesh.Release();
            }

            if (associatedData.ShadowMapPermutation != null)
            {
                lightingPlugin.RemoveShadowMap(associatedData.ShadowMapPermutation.ShadowMap);
                associatedData.ShadowMapPermutation = null;
            }

            if (associatedData.Light != null)
            {
                associatedData.Light = null;
            }

            associatedData.LightComponent.Tags.Remove(ShadowMapKey);
            associatedData.LightComponent.Tags.Remove(LightKey);

            if (associatedData.IconMesh != null)
            {
                renderPassEnumerator.RemoveMesh(associatedData.IconMesh);
                associatedData.IconMesh.Release();
                associatedData.IconMesh = null;
            }
        }

        public class AssociatedData
        {
            public LightComponent LightComponent;
            public TransformationComponent TransformationComponent;
            public EffectMesh EffectMesh;
            public Light Light;
            public ShadowMapPermutation ShadowMapPermutation;
            public EffectMesh IconMesh;

            public bool CurrentShadowMap;
            public LightType CurrentType;
        }
    }
}
