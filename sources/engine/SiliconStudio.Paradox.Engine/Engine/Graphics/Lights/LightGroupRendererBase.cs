// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Runtime.InteropServices;

using SiliconStudio.Core.Collections;
using SiliconStudio.Paradox.Effects.Shadows;

namespace SiliconStudio.Paradox.Effects.Lights
{
    public abstract class LightGroupRendererBase
    {
        protected LightGroupRendererBase()
        {
        }

        public byte LightType { get; protected set; }

        public bool AllocateLightMaxCount { get; protected set; }

        public int LightMaxCount { get; protected set; }

        public virtual void Initialize(RenderContext context)
        {
        }

        public abstract ILightShaderGenerator CreateShaderGenerator(LightComponentCollection lights, ILightShadowMapRenderer shadowMapRenderer);
    }
}