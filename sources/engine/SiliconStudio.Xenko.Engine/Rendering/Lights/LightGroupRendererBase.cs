// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using SiliconStudio.Core.Collections;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Rendering.Shadows;

namespace SiliconStudio.Xenko.Rendering.Lights
{
    /// <summary>
    /// Base class for light renderers.
    /// </summary>
    public abstract class LightGroupRendererBase
    {
        private static readonly Dictionary<Type, int> LightRendererIds = new Dictionary<Type, int>();

        protected LightGroupRendererBase()
        {
            int lightRendererId;
            lock (LightRendererIds)
            {
                if (!LightRendererIds.TryGetValue(GetType(), out lightRendererId))
                {
                    lightRendererId = LightRendererIds.Count + 1;
                    LightRendererIds.Add(GetType(), lightRendererId);
                }
            }

            LightRendererId = (byte)lightRendererId;
        }

        public bool IsEnvironmentLight { get; protected set; }

        public byte LightRendererId { get; private set; }

        public virtual void Initialize(RenderContext context)
        {
        }

        public virtual void Unload()
        {
        }

        public virtual void Reset()
        {
        }

        public virtual void SetViews(FastList<RenderView> views)
        {
            
        }

        public abstract void ProcessLights(ProcessLightsParameters parameters);

        public struct ProcessLightsParameters
        {
            public RenderDrawContext Context;

            // Information about the view
            public int ViewIndex;
            public RenderView View;
            public FastList<RenderView> Views;

            public LightComponentCollection LightCollection;
            public Type LightType;
            
            // Light range to process in LightCollection
            public int LightStart;
            public int LightEnd;

            public ShadowMapRenderer ShadowMapRenderer;

            public Dictionary<LightComponent, LightShadowMapTexture> ShadowMapTexturesPerLight;
        }

        public abstract void UpdateShaderPermutationEntry(ForwardLightingRenderFeature.LightShaderPermutationEntry shaderEntry);
    }
}