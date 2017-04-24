// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;
using SiliconStudio.Xenko.Rendering;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Games;
using SiliconStudio.Core;

namespace ScriptShader.Effects
{
    public class LightShaftsProcessor : EntityProcessor<LightShaftsProcessor.AssociatedData>
    {
        public static readonly PropertyKey<LightShaftsPlugin> LightShaftsPluginKey = new PropertyKey<LightShaftsPlugin>("LightShaftsPlugin", typeof(LightShaftsProcessor));

        private IRenderSystem renderSystem;
        private MainPlugin mainPlugin;
        private RenderTargetsPlugin mainTargetPlugin;
        private RenderPass lightShaftsPass;

        public LightShaftsProcessor(MainPlugin mainPlugin, RenderTargetsPlugin mainTargetPlugin, RenderPass lightShaftsPass)
            : base(new PropertyKey[] { LightShaftsComponent.Key, LightComponent.Key })
        {
            this.mainPlugin = mainPlugin;
            this.mainTargetPlugin = mainTargetPlugin;
            this.lightShaftsPass = lightShaftsPass;
        }

        protected override void OnSystemAdd()
        {
            renderSystem = Services.GetSafeServiceAs<IRenderSystem>();
        }

        protected override AssociatedData GenerateAssociatedData(Entity entity)
        {
            return new AssociatedData()
                {
                    LightShaftsComponent = entity.Get(LightShaftsComponent.Key),
                    LightComponent = entity.Get(LightComponent.Key),
                };
        }

        protected override void OnEntityAdded(Entity entity, AssociatedData data)
        {
            var shadowMapPermutation = data.LightComponent.Get(LightProcessor.ShadowMapKey);
            
            data.CurrentShadowMapPermutation = shadowMapPermutation;
            if (shadowMapPermutation != null)
            {
                data.LightShaftsPlugin = new LightShaftsPlugin("LightShaftsPlugin")
                    {
                        Debug = false,
                        RenderPass = lightShaftsPass,
                        ShadowMap = entity.Get(LightComponent.Key).Get(LightProcessor.ShadowMapKey).ShadowMap,
                        RenderTarget = mainTargetPlugin.RenderTarget,
                        DepthStencil = mainTargetPlugin.DepthStencil,
                        ViewParameters = mainPlugin.ViewParameters,
                    };
                data.LightShaftsPlugin.BoundingBoxes.AddRange(data.LightShaftsComponent.LightShaftsBoundingBoxes);

                renderSystem.RenderPassPlugins.Add(data.LightShaftsPlugin);
            }
        }

        protected override void OnEntityRemoved(Entity entity, AssociatedData data)
        {
            if (data.LightShaftsPlugin != null)
            {
                renderSystem.RenderPassPlugins.Remove(data.LightShaftsPlugin);
                data.LightShaftsPlugin = null;
            }
        }

        public override void Update()
        {
            foreach (var matchingEntity in enabledEntities)
            {
                var lightShaftsComponent = matchingEntity.Value.LightShaftsComponent;
                var lightShaftsPlugin = matchingEntity.Value.LightShaftsPlugin;

                // If shadow map changed, readd it
                if (matchingEntity.Value.CurrentShadowMapPermutation != matchingEntity.Value.LightComponent.Get(LightProcessor.ShadowMapKey))
                {
                    EntityReadd(matchingEntity.Key);
                }

                if (lightShaftsPlugin != null)
                {
                    lightShaftsPlugin.LightColor = lightShaftsComponent.Color;
                }
            }
        }

        public class AssociatedData
        {
            public LightShaftsComponent LightShaftsComponent;
            public LightShaftsPlugin LightShaftsPlugin;
            public LightComponent LightComponent;

            public ShadowMapPermutation CurrentShadowMapPermutation;
        }
    }
}
