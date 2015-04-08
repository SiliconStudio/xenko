// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;

using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.Effects.Lights;
using SiliconStudio.Paradox.Shaders;

namespace SiliconStudio.Paradox.Effects.Shadows
{
    public interface ILightShadowMapShaderData
    {
    }

    public interface ILightShadowMapShaderGroupData
    {
        void ApplyShader(ShaderMixinSource mixin);

        void SetShadowMapShaderData(int index, ILightShadowMapShaderData shaderData);

        void ApplyParameters(ParameterCollection parameters);
    }

    [Flags]
    public enum LightShadowType : byte
    {
        Cascade1 = 0x1,
        Cascade2 = 0x2,
        Cascade4 = 0x3,
        
        CascadeMask = 0x7,

        Debug    = 0x4,

        FilterMask = 0xF0,
    }

    /// <summary>
    /// An allocated shadow map texture associated to a light.
    /// </summary>
    public class LightShadowMapTexture
    {
        public delegate void ApplyShadowMapParametersDelegate(LightShadowMapTexture shadowMapTexture, ParameterCollection parameters);

        /// <summary>
        /// Initializes a new instance of the <see cref="LightShadowMapTexture" /> struct.
        /// </summary>
        /// <param name="lightComponent">The light component.</param>
        /// <param name="light">The light component.</param>
        /// <param name="shadowMap">The light.</param>
        /// <param name="size">The shadow map.</param>
        /// <param name="renderer">The renderer.</param>
        /// <exception cref="System.ArgumentNullException">
        /// lightComponent
        /// or
        /// light
        /// or
        /// shadowMap
        /// or
        /// renderer
        /// </exception>
        public LightShadowMapTexture()
        {
        }

        public LightComponent LightComponent { get; private set; }

        public IDirectLight Light { get; private set; }

        public LightShadowMap Shadow { get; private set; }

        public Type FilterType { get; private set; }

        public byte TextureId { get; internal set; }

        public LightShadowType ShadowType { get; internal set; }

        public int Size { get; private set; }

        public int CascadeCount { get; set; }

        public ShadowMapAtlasTexture Atlas { get; internal set; }

        public ILightShadowMapRenderer Renderer;

        public ILightShadowMapShaderData ShaderData;

        public void Initialize(LightComponent lightComponent, IDirectLight light, LightShadowMap shadowMap, int size, ILightShadowMapRenderer renderer)
        {
            if (lightComponent == null) throw new ArgumentNullException("lightComponent");
            if (light == null) throw new ArgumentNullException("light");
            if (shadowMap == null) throw new ArgumentNullException("shadowMap");
            if (renderer == null) throw new ArgumentNullException("renderer");
            LightComponent = lightComponent;
            Light = light;
            Shadow = shadowMap;
            Size = size;
            FilterType = Shadow.Filter == null || !Shadow.Filter.RequiresCustomBuffer() ? null : Shadow.Filter.GetType();
            Renderer = renderer;

            ShadowType = 0;
            switch (shadowMap.CascadeCount)
            {
                case LightShadowMapCascadeCount.OneCascade:
                    ShadowType |= LightShadowType.Cascade1;
                    break;
                case LightShadowMapCascadeCount.TwoCascades:
                    ShadowType |= LightShadowType.Cascade2;
                    break;
                case LightShadowMapCascadeCount.FourCascades:
                    ShadowType |= LightShadowType.Cascade4;
                    break;
            }

            // TODO: Add filter mask to ShadowType
            if (shadowMap.Debug)
            {
                ShadowType |= LightShadowType.Debug;
            }
        }

        public Rectangle GetRectangle(int i)
        {
            if (i < 0 || i > CascadeCount)
            {
                throw new ArgumentOutOfRangeException("i", "Must be in the range [0, CascadeCount[");
            }
            unsafe
            {
                fixed (void* ptr = &Rectangle0)
                {
                    return ((Rectangle*)ptr)[i];
                }
            }
        }

        public void SetRectangle(int i, Rectangle value)
        {
            if (i < 0 || i > CascadeCount)
            {
                throw new ArgumentOutOfRangeException("i", "Must be in the range [0, CascadeCount[");
            }
            unsafe
            {
                fixed (void* ptr = &Rectangle0)
                {
                    ((Rectangle*)ptr)[i] = value;
                }
            }
        }

        private Rectangle Rectangle0;

        private Rectangle Rectangle1;

        private Rectangle Rectangle2;

        private Rectangle Rectangle3;
    }
}