// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using SiliconStudio.Core.Collections;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Rendering.Lights;

namespace SiliconStudio.Xenko.Rendering.Shadows
{
    public abstract class LightShadowMapRendererBase : ILightShadowMapRenderer
    {
        public LightShadowMapRendererBase(ShadowMapRenderer parent)
        {
            ShadowMapRenderer = parent;
        }

        public ShadowMapRenderer ShadowMapRenderer { get; private set; }

        public abstract void Reset();

        public virtual LightShadowType GetShadowType(LightShadowMap shadowMap)
        {
            // TODO: MOVE THIS TO BASE TYPE
            var shadowType = (LightShadowType)0;
            switch (shadowMap.GetCascadeCount())
            {
                case 1:
                    shadowType |= LightShadowType.Cascade1;
                    break;
                case 2:
                    shadowType |= LightShadowType.Cascade2;
                    break;
                case 4:
                    shadowType |= LightShadowType.Cascade4;
                    break;
            }

            var pcfFilter = shadowMap.Filter as LightShadowMapFilterTypePcf;
            if (pcfFilter != null)
            {
                switch (pcfFilter.FilterSize)
                {
                    case LightShadowMapFilterTypePcfSize.Filter3x3:
                        shadowType |= LightShadowType.PCF3x3;
                        break;
                    case LightShadowMapFilterTypePcfSize.Filter5x5:
                        shadowType |= LightShadowType.PCF5x5;
                        break;
                    case LightShadowMapFilterTypePcfSize.Filter7x7:
                        shadowType |= LightShadowType.PCF7x7;
                        break;
                }
            }

            if (shadowMap.Debug)
            {
                shadowType |= LightShadowType.Debug;
            }
            return shadowType;
        }

        public abstract ILightShadowMapShaderGroupData CreateShaderGroupData(LightShadowType shadowType);

        public abstract bool CanRenderLight(IDirectLight light);

        public abstract void Collect(RenderContext context, LightShadowMapTexture lightShadowMap);

        public abstract void CreateRenderViews(LightShadowMapTexture shadowMapTexture, VisibilityGroup visibilityGroup);

        public virtual void ApplyViewParameters(RenderDrawContext context, ParameterCollection parameters, LightShadowMapTexture shadowMapTexture)
        {
        }

        public virtual LightShadowMapTexture CreateTexture(LightComponent lightComponent, IDirectLight light, int shadowMapSize)
        {
            var shadowMapTexture = ShadowMapRenderer.ShadowMapTextures.Add();
            shadowMapTexture.Initialize(lightComponent, light, light.Shadow, shadowMapSize, this);
            shadowMapTexture.CascadeCount = light.Shadow.GetCascadeCount();
            return shadowMapTexture;
        }
    }

    public abstract class CascadeShadowMapRendererBase : LightShadowMapRendererBase
    {
        public CascadeShadowMapRendererBase(ShadowMapRenderer parent) : base(parent)
        {
        }

        public override void CreateRenderViews(LightShadowMapTexture shadowMapTexture, VisibilityGroup visibilityGroup)
        {
            for (int cascadeIndex = 0; cascadeIndex < shadowMapTexture.CascadeCount; cascadeIndex++)
            {
                // Allocate shadow render view
                var shadowRenderView = ShadowMapRenderer.ShadowRenderViews.Add();
                shadowRenderView.RenderView = ShadowMapRenderer.CurrentView;
                shadowRenderView.ShadowMapTexture = shadowMapTexture;
                shadowRenderView.Rectangle = shadowMapTexture.GetRectangle(cascadeIndex);

                // Compute view parameters
                GetCascadeViewParameters(shadowMapTexture, cascadeIndex, out shadowRenderView.View, out shadowRenderView.Projection);
                Matrix.Multiply(ref shadowRenderView.View, ref shadowRenderView.Projection, out shadowRenderView.ViewProjection);

                // Add the render view for the current frame
                ShadowMapRenderer.RenderSystem.Views.Add(shadowRenderView);

                // Collect objects in shadow views
                visibilityGroup.Collect(shadowRenderView);
            }
        }

        public abstract void GetCascadeViewParameters(LightShadowMapTexture shadowMapTexture, int cascadeIndex, out Matrix view, out Matrix projection);
    }
}