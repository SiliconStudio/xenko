// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using SiliconStudio.Core.Collections;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Rendering.Shadows;
using SiliconStudio.Xenko.Shaders;

namespace SiliconStudio.Xenko.Rendering.Lights
{
    public abstract class LightShaderGroup
    {
        protected LightShaderGroup()
        {
        }

        protected LightShaderGroup(ShaderSource mixin)
        {
            ShaderSource = mixin;
        }

        public ShaderSource ShaderSource { get; protected set; }

        public bool HasEffectPermutations { get; protected set; } = false;

        public virtual void UpdateLayout(string compositionName)
        {
        }

        public virtual void Reset()
        {
        }

        public virtual void ApplyEffectPermutations(RenderEffect renderEffect)
        {
        }

        public virtual void ApplyViewParameters(RenderDrawContext context, int viewIndex, ParameterCollection parameters)
        {
        }

        public virtual void ApplyDrawParameters(RenderDrawContext context, int viewIndex, ParameterCollection parameters, ref BoundingBoxExt boundingBox)
        {
        }
    }

    public abstract class LightShaderGroupDynamic : LightShaderGroup
    {
        /// <summary>
        /// List of all available lights.
        /// </summary>
        protected FastListStruct<LightDynamicEntry> Lights = new FastListStruct<LightDynamicEntry>(8);

        protected LightRange[] LightRanges;

        public RenderDrawContext Context { get; }

        /// <summary>
        /// List of lights selected for this rendering.
        /// </summary>
        protected FastListStruct<LightDynamicEntry> CurrentLights = new FastListStruct<LightDynamicEntry>(8);

        public ILightShadowMapShaderGroupData ShadowGroup { get; }

        public int LightCurrentCount { get; private set; }

        public int LightLastCount { get; private set; }

        protected LightShaderGroupDynamic(RenderDrawContext context, ILightShadowMapShaderGroupData shadowGroup)
        {
            Context = context;
            ShadowGroup = shadowGroup;
        }

        public override void Reset()
        {
            base.Reset();
            Lights.Clear();
            LightLastCount = LightCurrentCount;
            LightCurrentCount = 0;
        }

        public void SetViewCount(int viewCount)
        {
            Array.Resize(ref LightRanges, viewCount);

            // Reset ranges
            for (var i = 0; i < viewCount; ++i)
                LightRanges[i] = new LightRange(0, 0);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="viewIndex"></param>
        /// <param name="lightCount"></param>
        /// <returns>The number of lights accepted.</returns>
        public int AddView(int viewIndex, int lightCount)
        {
            LightRanges[viewIndex] = new LightRange(Lights.Count, Lights.Count + lightCount);
            LightCurrentCount = Math.Max(LightCurrentCount, ComputeLightCount(lightCount));

            return Math.Min(LightCurrentCount, lightCount);
        }

        public virtual int ComputeLightCount(int lightCount)
        {
            // Shadows: return exact number
            // TODO: Only for PerView; PerDraw could be little bit more loose to avoid extra permutations
            if (ShadowGroup != null)
            {
                return lightCount;
            }

            // Use next power of two
            lightCount = MathUtil.NextPowerOfTwo(lightCount);

            // Make sure it is at least 8 to avoid unecessary permutations
            lightCount = Math.Max(lightCount, 8);

            return lightCount;
        }

        /// <summary>
        /// Try to add light to this group (returns false if not possible).
        /// </summary>
        /// <param name="light"></param>
        /// <param name="shadowMapTexture"></param>
        /// <returns></returns>
        public bool AddLight(LightComponent light, LightShadowMapTexture shadowMapTexture)
        {
            Lights.Add(new LightDynamicEntry(light, shadowMapTexture));
            return true;
        }

        public override void UpdateLayout(string compositionName)
        {
            base.UpdateLayout(compositionName);
            ShadowGroup?.UpdateLayout(compositionName);

            if (LightLastCount != LightCurrentCount)
            {
                ShadowGroup?.UpdateLightCount(LightLastCount, LightCurrentCount);
                UpdateLightCount();
            }
        }

        protected virtual void UpdateLightCount()
        {

        }

        public override void ApplyViewParameters(RenderDrawContext context, int viewIndex, ParameterCollection parameters)
        {
            base.ApplyViewParameters(context, viewIndex, parameters);
            ShadowGroup?.ApplyViewParameters(context, parameters, CurrentLights);
        }

        public override void ApplyDrawParameters(RenderDrawContext context, int viewIndex, ParameterCollection parameters, ref BoundingBoxExt boundingBox)
        {
            base.ApplyDrawParameters(context, viewIndex, parameters, ref boundingBox);
            ShadowGroup?.ApplyDrawParameters(context, parameters, CurrentLights, ref boundingBox);
        }

        public struct LightRange
        {
            public readonly int Start;
            public readonly int End;

            public LightRange(int start, int end)
            {
                Start = start;
                End = end;
            }

            public override string ToString()
            {
                return $"LightRange {Start}..{End}";
            }
        }
    }

    public struct LightDynamicEntry
    {
        public readonly LightComponent Light;
        public readonly LightShadowMapTexture ShadowMapTexture;

        public LightDynamicEntry(LightComponent light, LightShadowMapTexture shadowMapTexture)
        {
            Light = light;
            ShadowMapTexture = shadowMapTexture;
        }
    }
}