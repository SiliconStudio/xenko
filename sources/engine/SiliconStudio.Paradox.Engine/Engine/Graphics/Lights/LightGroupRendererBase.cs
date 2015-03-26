// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Collections.Generic;

namespace SiliconStudio.Paradox.Effects.Lights
{
    public abstract class LightGroupRendererBase
    {
        protected LightGroupRendererBase()
        {
        }

        public abstract bool IsDirectLight { get; }

        public abstract List<LightShaderGroup> PrepareLights(RenderContext context, LightComponentCollection lights, bool isLightWithShadow);
    }
}