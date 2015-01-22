// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.Linq;

using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.DataModel;
using SiliconStudio.Paradox.Effects.Lights;
using SiliconStudio.Paradox.Effects.Processors;
using SiliconStudio.Paradox.Engine;
using SiliconStudio.Paradox.EntityModel;
using SiliconStudio.Paradox.Graphics;
using SiliconStudio.Paradox.Shaders;

namespace SiliconStudio.Paradox.Effects.Renderers
{
    /// <summary>
    /// TODO: Evaluate if it would be possible to split this class with support for different lights instead of a big fat class
    /// TODO: Refactor this class
    /// </summary>
    internal class LightForwardModelRenderer
    {
        private readonly List<EntityLightShadow> lights = new List<EntityLightShadow>(); 

        public LightForwardModelRenderer(IServiceRegistry services)
        {
            if (services == null) throw new ArgumentNullException("services");
            Services = services;
        }

        public IServiceRegistry Services { get; private set; }


        /// <summary>
        /// Filter out the inactive lights.
        /// </summary>
        /// <param name="context">The render context.</param>
        public void PreRender(RenderContext context)
        {
            // get the lightprocessor
            var entitySystem = Services.GetServiceAs<EntitySystem>();
            var lightProcessor = entitySystem.GetProcessor<LightShadowProcessor>();
            if (lightProcessor == null)
                return;

            lights.Clear();

            foreach (var light in lightProcessor.Lights)
            {
                if (!light.Value.Light.Deferred && light.Value.Light.Enabled)
                {
                    lights.Add(light.Value);
                }
            }
        }

        /// <summary>
        /// Clear the light lists.
        /// </summary>
        /// <param name="context">The render context.</param>
        public void PostRender(RenderContext context)
        {
        }

        private ShaderSource[] directLightGroups;

        private readonly ParameterKey<int> countDirectionalKey = ParameterKeys.New(0);


        /// <summary>
        /// Update light lists and choose the new light configuration.
        /// </summary>
        /// <param name="context">The render context.</param>
        /// <param name="renderMesh">The current RenderMesh (the same as <seealso cref="PostEffectUpdate"/>)</param>
        public void PreEffectUpdate(RenderContext context, RenderMesh renderMesh)
        {
            // WARNING: This is just a dirty code to reactivate and test Directional Lighting. 
            // BUT This should be written with pluggability in mind
            int countDirectional = lights.Count(light => (light.Light.Type is LightDirectional));
            int previousCountDirectional;
            renderMesh.Parameters.TryGet(countDirectionalKey, out previousCountDirectional);
            bool isNewCount = previousCountDirectional != countDirectional;

            if (countDirectional == 0)
            {
                if (isNewCount)
                {
                    renderMesh.Parameters.Set(countDirectionalKey, countDirectional);
                    renderMesh.Parameters.Set(LightingKeys.DirectLightGroups, null);
                }
                return;
            }

            if (isNewCount)
            {
                var shaderSources = new List<ShaderSource>();
                shaderSources.Add(new ShaderClassSource("LightDirectionalGroup", countDirectional));
                directLightGroups = shaderSources.ToArray();
                renderMesh.Parameters.Set(countDirectionalKey, countDirectional);
                renderMesh.Parameters.Set(LightingKeys.DirectLightGroups, directLightGroups);
            }

            var lightDirections = new Vector3[countDirectional];
            var lightColors = new Color3[countDirectional];

            var viewMatrix = context.CurrentPass.Parameters.Get(TransformationKeys.View);

            int directLightIndex = 0;
            foreach (var light in lights)
            {
                if (!(light.Light.Type is LightDirectional))
                {
                    continue;
                }

                var lightDir = light.Light.Direction;

                Matrix worldView;
                Vector3 direction;
                Matrix.Multiply(ref light.Entity.Transformation.WorldMatrix, ref viewMatrix, out worldView);
                Vector3.TransformNormal(ref lightDir, ref worldView, out direction);

                lightDirections[directLightIndex] = direction;
                lightColors[directLightIndex] = light.Light.Color.ToLinear() * light.Light.Intensity;
            }

            renderMesh.Parameters.Set(DirectLightGroupKeys.GetParameterKey(DirectLightGroupKeys.LightCount, 0), countDirectional);
            renderMesh.Parameters.Set(DirectLightGroupKeys.GetParameterKey(LightDirectionalGroupKeys.LightDirectionsVS, 0), lightDirections);
            renderMesh.Parameters.Set(DirectLightGroupKeys.GetParameterKey(LightDirectionalGroupKeys.LightColor, 0), lightColors);
        }

        /// <summary>
        /// Update the light values of the shader.
        /// </summary>
        /// <param name="context">The render context.</param>
        /// <param name="renderMesh">The current RenderMesh (the same as <seealso cref="PreEffectUpdate"/>)</param>
        public void PostEffectUpdate(RenderContext context, RenderMesh renderMesh)
        {
            
        }
    }
}
