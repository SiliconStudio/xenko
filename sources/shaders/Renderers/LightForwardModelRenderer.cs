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
    public class LightForwardModelRenderer
    {
        private readonly List<EntityLightShadow> lights = new List<EntityLightShadow>(); 

        public LightForwardModelRenderer(IServiceRegistry services)
        {
            if (services == null) throw new ArgumentNullException("services");
            Services = services;
            previousCountDirectional = -1;
        }

        public IServiceRegistry Services { get; private set; }

        public void EnableLights(ModelRenderer modelRenderer, bool isEnabled)
        {
            if (modelRenderer == null) throw new ArgumentNullException("modelRenderer");
            if (isEnabled)
            {
                modelRenderer.PreRender.Add(this.PreRender);
                modelRenderer.PostRender.Add(this.PostRender);
                modelRenderer.PreEffectUpdate.Add(this.PreEffectUpdate);
                modelRenderer.PostEffectUpdate.Add(this.PostEffectUpdate);
            }
            else
            {
                modelRenderer.PreRender.Remove(this.PreRender);
                modelRenderer.PostRender.Remove(this.PostRender);
                modelRenderer.PreEffectUpdate.Remove(this.PreEffectUpdate);
                modelRenderer.PostEffectUpdate.Remove(this.PostEffectUpdate);
            }
        }

        private bool isNewCount;

        private ShaderSource[] directLightGroups;

        private int previousCountDirectional;

        private readonly ParameterKey<int> countDirectionalKey = ParameterKeys.New(0);

        private ParameterKey<int> directLightGroupKeysLightCount;
        private ParameterKey<Vector3[]> directLightGroupKeysLightDirectionsVS;
        private ParameterKey<Color3[]> directLightGroupKeysLightColor;

        private Vector3[] lightDirections;

        private Color3[] lightColors;

        private int countDirectional;

        /// <summary>
        /// Filter out the inactive lights.
        /// </summary>
        /// <param name="context">The render context.</param>
        private void PreRender(RenderContext context)
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

            // TEMP: this should be set dynamically by group
            directLightGroupKeysLightCount = DirectLightGroupKeys.GetParameterKey(DirectLightGroupKeys.LightCount, 0);
            directLightGroupKeysLightDirectionsVS = DirectLightGroupKeys.GetParameterKey(LightDirectionalGroupKeys.LightDirectionsVS, 0);
            directLightGroupKeysLightColor = DirectLightGroupKeys.GetParameterKey(LightDirectionalGroupKeys.LightColor, 0);

            // WARNING: This is just a dirty code to reactivate and test Directional Lighting. 
            // BUT This should be written with pluggability in mind
            countDirectional = lights.Count(light => (light.Light.Type is LightDirectional));
            isNewCount = previousCountDirectional != countDirectional;

            if (countDirectional == 0)
            {
                if (isNewCount)
                {
                    directLightGroups = null;
                }
                return;
            }

            if (isNewCount)
            {
                var shaderSources = new List<ShaderSource>
                {
                    new ShaderClassSource("LightDirectionalGroup", 8)
                };
                directLightGroups = shaderSources.ToArray();

                lightDirections = new Vector3[countDirectional];
                lightColors = new Color3[countDirectional];
            }

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

        }

        /// <summary>
        /// Clear the light lists.
        /// </summary>
        /// <param name="context">The render context.</param>
        private void PostRender(RenderContext context)
        {
        }

        /// <summary>
        /// Update light lists and choose the new light configuration.
        /// </summary>
        /// <param name="context">The render context.</param>
        /// <param name="renderMesh">The current RenderMesh (the same as <seealso cref="PostEffectUpdate"/>)</param>
        private void PreEffectUpdate(RenderContext context, RenderMesh renderMesh)
        {
            if (isNewCount)
            {
                // Only set this keys when needed, as they are used for compilation parameters, they should not be updated 
                // if they don't change
                renderMesh.Parameters.Set(directLightGroupKeysLightCount, countDirectional);
                renderMesh.Parameters.Set(LightingKeys.DirectLightGroups, directLightGroups);
            }

            renderMesh.Parameters.Set(directLightGroupKeysLightDirectionsVS, lightDirections);
            renderMesh.Parameters.Set(directLightGroupKeysLightColor, lightColors);
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
