// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Paradox.Effects.Processors;
using SiliconStudio.Paradox.Shaders;

namespace SiliconStudio.Paradox.Effects.Lights
{
    public abstract class DirectLightGroupRenderProcessorBase
    {
        protected DirectLightGroupRenderProcessorBase()
        {
            Parameters = new ParameterCollection();
        }

        public abstract ShaderSource PrepareLights(RenderContext context, LightComponentCollection lights);

        public abstract void PrepareLightsWithShadows(RenderContext context, LightComponentCollection lights);

        public ParameterCollection Parameters { get; private set; }
    }
}