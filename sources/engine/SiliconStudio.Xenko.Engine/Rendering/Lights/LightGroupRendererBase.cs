// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;

using SiliconStudio.Core.Collections;
using SiliconStudio.Xenko.Rendering.Shadows;
using SiliconStudio.Xenko.Shaders;

namespace SiliconStudio.Xenko.Rendering.Lights
{
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

        public bool AllocateLightMaxCount { get; protected set; }

        public int LightMaxCount { get; protected set; }

        public bool CanHaveShadows { get; protected set; }

        public virtual void Initialize(RenderContext context)
        {
        }

        public abstract LightShaderGroup CreateLightShaderGroup(string compositionName, int lightMaxCount, ILightShadowMapShaderGroupData shadowGroup);
    }
}