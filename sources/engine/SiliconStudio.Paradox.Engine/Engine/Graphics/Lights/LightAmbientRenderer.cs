// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Collections.Generic;

using SiliconStudio.Paradox.Shaders;

namespace SiliconStudio.Paradox.Effects.Lights
{
    /// <summary>
    /// Light renderer for <see cref="LightAmbient"/>.
    /// </summary>
    public class LightAmbientRenderer : LightGroupRendererBase
    {
        private readonly List<LightShaderGroup> currentLightGroups = new List<LightShaderGroup>();

        private readonly LightShaderGroup lightShaderGroup;

        public LightAmbientRenderer()
        {
            lightShaderGroup = new LightShaderGroup(new ShaderClassSource("LightSimpleAmbient"));
        }

        public override bool IsDirectLight
        {
            get
            {
                return false;
            }
        }

        public override List<LightShaderGroup> PrepareLights(RenderContext context, LightComponentCollection lights)
        {
            var count = lights.Count;
            currentLightGroups.Clear();
            for (int i = 0; i < count; i++)
            {
                var lightComponent = lights[i];
                var light = (LightAmbient)lightComponent.Type;

                var color = light.ComputeColor(lightComponent.Intensity);

                lightShaderGroup.Parameters.Set(LightSimpleAmbientKeys.AmbientLight, color);
                currentLightGroups.Add(lightShaderGroup);
            }
            return currentLightGroups;
        }
    }
}