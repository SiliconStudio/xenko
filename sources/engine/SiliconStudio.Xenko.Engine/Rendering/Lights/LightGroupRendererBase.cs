// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System;
using System.Collections.Generic;
using SiliconStudio.Core;
using SiliconStudio.Core.Collections;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Rendering.Shadows;

namespace SiliconStudio.Xenko.Rendering.Lights
{
    /// <summary>
    /// Base class for light renderers.
    /// </summary>
    [DataContract(Inherited = true, DefaultMemberMode = DataMemberMode.Never)]
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

        public abstract Type[] LightTypes { get; }

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

            // Current renderers in this group
            public LightGroupRendererBase[] Renderers;
            // Index into the Renderers array
            public int RendererIndex;
            
            public LightComponentCollection LightCollection;
            public Type LightType;
            
            // Light range to process in LightCollection
            // The light group renderer should remove lights it processes
            public List<int> LightIndices;

            public IShadowMapRenderer ShadowMapRenderer;

            public Dictionary<LightComponent, LightShadowMapTexture> ShadowMapTexturesPerLight;
        }

        public abstract void UpdateShaderPermutationEntry(ForwardLightingRenderFeature.LightShaderPermutationEntry shaderEntry);

        public virtual void PrepareResources(RenderDrawContext drawContext)
        {
            
        }
    }
}
